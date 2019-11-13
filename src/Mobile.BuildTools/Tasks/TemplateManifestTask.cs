using System;
using System.IO;
using Mobile.BuildTools.Generators;
using Mobile.BuildTools.Generators.Manifests;
using Mobile.BuildTools.Logging;
using Mobile.BuildTools.Tasks.Utils;

namespace Mobile.BuildTools.Tasks
{
    public class TemplateManifestTask : Microsoft.Build.Utilities.Task
    {
        public string Prefix { get; set; }

        public string Token { get; set; }

        public string ProjectDirectory { get; set; }

        public string ManifestOutputPath { get; set; }

        public string SdkShortFrameworkIdentifier { get; set; }
        public string[] ReferenceAssemblyPaths { get; set; }

        public string DebugOutput { get; set; }

        public override bool Execute()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ManifestOutputPath))
                {
                    Log.LogMessage($"No Template path specified for {SdkShortFrameworkIdentifier}");
                }
                else if (File.Exists(ManifestOutputPath))
                {
                    Log.LogMessage($"Generating '{Path.GetFileName(ManifestOutputPath)}'");
                    bool.TryParse(DebugOutput, out var debug);
                    IGenerator generator = null;
                    switch(SdkShortFrameworkIdentifier.GetTargetPlatform())
                    {
                        case Platform.iOS:
                            generator = new TemplatedPlistGenerator()
                            {
                                Prefix = Prefix,
                                Token = Token,
                                SdkShortFrameworkIdentifier = SdkShortFrameworkIdentifier,
                                ProjectDirectory = ProjectDirectory,
                                ManifestOutputPath = ManifestOutputPath,
                                DebugOutput = debug,
                                Log = (BuildHostLoggingHelper)Log,
                            };
                            break;
                        case Platform.Android:
                            new TemplatedAndroidAppManifestGenerator(ReferenceAssemblyPaths)
                            {
                                Prefix = Prefix,
                                Token = Token,
                                SdkShortFrameworkIdentifier = SdkShortFrameworkIdentifier,
                                ProjectDirectory = ProjectDirectory,
                                ManifestOutputPath = ManifestOutputPath,
                                DebugOutput = debug,
                                Log = (BuildHostLoggingHelper)Log,
                            };
                            break;
                        default:
                            Log.LogError($"Platform {SdkShortFrameworkIdentifier} is unsupported");
                            break;
                    }

                    generator?.Execute();
                }
                else
                {
                    Log.LogWarning($"Could not locate template manifest at '{ManifestOutputPath}'");
                }
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
            }

            return !Log.HasLoggedErrors;
        }
    }
}