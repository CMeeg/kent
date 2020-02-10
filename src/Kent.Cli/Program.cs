using PowerArgs;

namespace Kent.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Args.InvokeAction<KentActions>(args);
        }
    }
}
