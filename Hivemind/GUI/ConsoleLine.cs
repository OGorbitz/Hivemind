namespace Hivemind.GUI
{
    internal class ConsoleLine
    {
        public bool Cursor;
        public int Duration;
        public string Text;

        public ConsoleLine(string t, int d, bool c)
        {
            Text = t;
            Duration = d;
            Cursor = c;
        }
    }
}