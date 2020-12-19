using System;

namespace Hivemind
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Hivemind())
                game.Run();
        }
    }
}
