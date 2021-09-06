// Copyright 2021 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using NuGet.Versioning;
using Nuke.Common;
using Nuke.Common.Tools.Npm;

namespace Nuke.MSBuildTasks
{
    [UsedImplicitly]
    public class InstallNpmToolsTask : ContextAwareTask
    {
        [Microsoft.Build.Framework.Required]
        public ITaskItem[] NpmTools { get; set; }

        protected override bool ExecuteInner()
        {
            var installedNpmTools = NpmTasks.Npm("list -g")
                .Select(x => x.Text.Split(' '))
                .Where(x => x.Length == 2)
                .Select(x => x.Last())
                .ToDictionary(
                    x => x.Substring(startIndex: 0, x.LastIndexOf("@", StringComparison.Ordinal)),
                    x => NuGetVersion.Parse(x.Substring(x.LastIndexOf("@", StringComparison.Ordinal) + 1)));

            foreach (var tool in NpmTools)
            {
                var package = tool.ItemSpec;
                var version = tool.GetMetadata("Version").NotNull("version != null");

                if (installedNpmTools.GetValueOrDefault(package)?.ToString() == version)
                    continue;

                NpmTasks.Npm($"install {package}@{version} -g");
            }

            return true;
        }
    }
}
