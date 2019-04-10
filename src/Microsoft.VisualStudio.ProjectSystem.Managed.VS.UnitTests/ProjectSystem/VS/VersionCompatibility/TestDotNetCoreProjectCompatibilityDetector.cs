﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.IO;
using Microsoft.VisualStudio.ProjectSystem.VS.Interop;
using Microsoft.VisualStudio.ProjectSystem.VS.UI;
using Microsoft.VisualStudio.ProjectSystem.VS.Utilities;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ProjectSystem.VS.VersionCompatibility
{
    internal class TestDotNetCoreProjectCompatibilityDetector : DotNetCoreProjectCompatibilityDetector
    {
        private readonly bool _hasNewProjects;

        public TestDotNetCoreProjectCompatibilityDetector(Lazy<IProjectServiceAccessor> projectAccessor,
                                                          Lazy<IDialogServices> dialogServices,
                                                          Lazy<IProjectThreadingService> threadHandling,
                                                          Lazy<IVsShellUtilitiesHelper> vsShellUtilitiesHelper,
                                                          Lazy<IFileSystem> fileSystem,
                                                          Lazy<IHttpClient> httpClient,
                                                          IVsService<SVsUIShell, IVsUIShell> vsUIShellService,
                                                          IVsService<SVsSettingsPersistenceManager, ISettingsManager> settingsManagerService,
                                                          IVsService<SVsSolution, IVsSolution> vsSolutionService,
                                                          IVsService<SVsAppId, IVsAppId> vsAppIdService,
                                                          IVsService<SVsShell, IVsShell> vsShellService,
                                                          bool hasNewProjects = false) :
            base(projectAccessor, dialogServices, threadHandling, vsShellUtilitiesHelper, fileSystem, httpClient, vsUIShellService, settingsManagerService, vsSolutionService, vsAppIdService, vsShellService)
        {
            _hasNewProjects = hasNewProjects;
        }

        protected override bool IsNewlyCreated(UnconfiguredProject project) => _hasNewProjects;
    }
}
