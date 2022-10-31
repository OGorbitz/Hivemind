using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hivemind.GUI
{
    internal class ConsoleText
    {
        //Cursor flicking speed in milliseconds
        private readonly int CursorDuration;
        private readonly List<ConsoleLine> Lines = new List<ConsoleLine>();
        private readonly bool Loops;
        private int CurrentLine;

        private TimeSpan LastMark;


        public ConsoleText(int c, bool l)
        {
            CursorDuration = c;
            Loops = l;
        }

        public void AddLine(ConsoleLine l)
        {
            Lines.Add(l);
        }

        public void AddLine(string t, int d, bool c)
        {
            Lines.Add(new ConsoleLine(t, d, c));
        }

        //returns num of lines currently being displayed
        public string GetLines(int num)
        {
            var gameTime = Hivemind.CurrentGameTime;
            if (LastMark == TimeSpan.MinValue)
                LastMark = gameTime.TotalGameTime;

            var result = "";

            var elapsed = (gameTime.TotalGameTime - LastMark).Milliseconds +
                          (gameTime.TotalGameTime - LastMark).Seconds * 1000;
            if (elapsed > Lines[CurrentLine].Duration)
            {
                if (CurrentLine + 1 < Lines.Count)
                {
                    CurrentLine++;
                }
                else if (Loops)
                {
                    CurrentLine = 0;
                }
                LastMark = gameTime.TotalGameTime;
            }

            if (Lines.Count == 0)
                return "";


            for (var i = num; i > 0; i--)
            {
                var n = CurrentLine - i;
                if (n < 0)
                {
                    if (Loops)
                        n = Lines.Count + n;
                    else
                        continue;
                }

                result += Lines[n].Text + "\n";
            }

            if (Lines[CurrentLine].Cursor &&
                gameTime.TotalGameTime.Milliseconds % CursorDuration / (float) CursorDuration > 0.5f)
                result += Lines[CurrentLine].Text + "_";
            else
                result += Lines[CurrentLine].Text;

            return result;
        }
    }

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