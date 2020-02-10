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
    }
}
