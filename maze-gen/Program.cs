using CommandLine;

namespace PlayersWorlds.Maps {
    partial class MainClass {
        public static void Main(string[] args) =>
            Parser.Default.ParseArguments<
            GenerateCommand, ParseCommand, RunCommand, PerfRunCommand,
            UseCaseCommand>(args)
                .MapResult(
                (GenerateCommand opts) => opts.Run(),
                (ParseCommand opts) => opts.Run(),
                (RunCommand opts) => opts.Run(),
                (PerfRunCommand opts) => opts.Run(),
                (UseCaseCommand opts) => opts.Run(),
                errs => 1);
    }
}
