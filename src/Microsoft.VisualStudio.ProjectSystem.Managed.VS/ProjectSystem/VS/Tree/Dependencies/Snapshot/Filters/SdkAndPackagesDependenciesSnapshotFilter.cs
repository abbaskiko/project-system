﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.CrossTarget;
using Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.Subscriptions;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.Snapshot.Filters
{
    /// <summary>
    /// Sdk nodes are actually packages and their hierarchy of dependencies is resolved from
    /// NuGet's assets json file. However Sdk themselves are brought by DesignTime build for rules
    /// SdkReference. This filter matches Sdk to their corresponding NuGet package and sets  
    /// of top level sdk dependencies from the package. Packages are invisible to avoid visual
    /// duplication and confusion.
    /// </summary>
    [Export(typeof(IDependenciesSnapshotFilter))]
    [AppliesTo(ProjectCapability.DependenciesTree)]
    [Order(Order)]
    internal class SdkAndPackagesDependenciesSnapshotFilter : DependenciesSnapshotFilterBase
    {
        public const int Order = 110;

        public override IDependency BeforeAdd(
            string projectPath,
            ITargetFramework targetFramework,
            IDependency dependency,
            ImmutableDictionary<string, IDependency>.Builder worldBuilder,
            ImmutableHashSet<IDependency>.Builder topLevelBuilder,
            IReadOnlyDictionary<string, IProjectDependenciesSubTreeProvider> subTreeProviders,
            IImmutableSet<string> projectItemSpecs,
            out bool filterAnyChanges)
        {
            filterAnyChanges = false;

            if (!dependency.TopLevel)
            {
                return dependency;
            }

            if (dependency.Flags.Contains(DependencyTreeFlags.SdkSubTreeNodeFlags))
            {
                // Try to find a package dependency with the same name
                string packageId = Dependency.GetID(targetFramework, PackageRuleHandler.ProviderTypeString, modelId: dependency.Name);

                if (worldBuilder.TryGetValue(packageId, out IDependency package) && package.Resolved)
                {
                    filterAnyChanges = true;
                    return dependency.ToResolved(
                        schemaName: ResolvedSdkReference.SchemaName,
                        dependencyIDs: package.DependencyIDs);
                }
            }
            else if (dependency.Flags.Contains(DependencyTreeFlags.PackageNodeFlags) && dependency.Resolved)
            {
                // Try to find an SDK dependency with the same name
                string sdkId = Dependency.GetID(targetFramework, SdkRuleHandler.ProviderTypeString, modelId: dependency.Name);

                if (worldBuilder.TryGetValue(sdkId, out IDependency sdk))
                {
                    // We have an SDK dependency for this package. Such dependencies, when implicit, are created
                    // as unresolved by SdkRuleHandler, and are only marked resolved here once we have resolved the
                    // corresponding package.
                    //
                    // Set to resolved, and copy dependencies.

                    filterAnyChanges = true;
                    sdk = sdk.ToResolved(
                        schemaName: ResolvedSdkReference.SchemaName,
                        dependencyIDs: dependency.DependencyIDs);

                    worldBuilder.Remove(sdk.Id);
                    worldBuilder.Add(sdk.Id, sdk);
                    topLevelBuilder.Remove(sdk);
                    topLevelBuilder.Add(sdk);
                }
            }

            return dependency;
        }

        public override bool BeforeRemove(
            string projectPath,
            ITargetFramework targetFramework,
            IDependency dependency,
            ImmutableDictionary<string, IDependency>.Builder worldBuilder,
            ImmutableHashSet<IDependency>.Builder topLevelBuilder,
            out bool filterAnyChanges)
        {
            filterAnyChanges = false;

            if (dependency.TopLevel && 
                dependency.Resolved && 
                dependency.Flags.Contains(DependencyTreeFlags.PackageNodeFlags))
            {
                // Try to find an SDK dependency with the same name
                string sdkId = Dependency.GetID(targetFramework, SdkRuleHandler.ProviderTypeString, modelId: dependency.Name);

                if (worldBuilder.TryGetValue(sdkId, out IDependency sdk))
                {
                    // We are removing the package dependency related to this SDK dependency
                    // and must undo the changes made above in BeforeAdd.
                    //
                    // Set to unresolved, and clear dependencies.

                    filterAnyChanges = true;
                    sdk = sdk.ToUnresolved(
                        schemaName: SdkReference.SchemaName,
                        dependencyIDs: ImmutableList<string>.Empty);

                    worldBuilder.Remove(sdk.Id);
                    worldBuilder.Add(sdk.Id, sdk);
                    topLevelBuilder.Remove(sdk);
                    topLevelBuilder.Add(sdk);
                }
            }

            return true;
        }
    }
}
