using PowerArgs;

namespace Kent.Cli
{
    internal class ExportArgs
    {
        // TODO: Use a SemVer value type?
        [ArgRequired]
        [ArgShortcut("-v")]
        [ArgDescription("The SemVer version of the exported package.")]
        public string Version { get; set; }

        [ArgRequired]
        [ArgShortcut("-out")]
        [ArgDescription("The output directory for the exported package.")]
        public string OutputDirectory { get; set; }
    }
}
