using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlitchWin
{
    public class StringTokenizer : IEnumerable<string>, IEnumerator<string>
    {
        public string Current { get; private set; }

        internal string String { get; }

        internal int Index { get; private set; }

        private static readonly char[] WhiteSpaceChars = Enumerable.Range(char.MinValue, char.MaxValue)
            .Select(e => (char)e)
            .Where(c => c == 32 || c >= 9 && c <= 13 || (c == 160 || c == 133))
            .ToArray();

        public StringTokenizer(string from)
        {
            String = from;
            Current = null;
            Index = 0;
        }

        public IEnumerator<string> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        public bool MoveNext()
        {
            if (Index == -1) return false;

            while (char.IsWhiteSpace(String[Index]))
            {
                Index++;
                if (Index >= String.Length) return false;
            }
            int newIndex;
            // support quoted text
            if (String[Index] == '"')
            {
                Index++;//remove initial quote
                newIndex = String.IndexOf('"', Index+1);
            }
            else
            {
                newIndex = String.IndexOfAny(WhiteSpaceChars, Index);
            }
            Current = String.Substring(Index, (newIndex == -1 ? String.Length : newIndex) - Index);//substring(startIndex,length)
            // remove closing quote
            if (newIndex != -1 && newIndex < String.Length && String[newIndex] == '"')
            {
                Index = newIndex + 1;
            }
            else
            {
                Index = newIndex;
            }
            return true;
        }

        /// <summary>
        /// Assigns the next element in the tokenizer to a string
        /// </summary>
        /// <param name="str">the string to assign</param>
        /// <returns>true if successful</returns>
        public bool Next(out string str)
        {
            var b = MoveNext();
            str = Current;
            return b;
        }

        public string Remaining()
        {
            var tokens = new StringBuilder();
            foreach (var token in this)
            {
                tokens.Append(' ').Append(token);
            }

            return tokens.Length == 0 ? "" : tokens.ToString().Substring(1); // remove first space
        }

        public StringTokenizer Clone()
        {
            return Index == -1
                ? new StringTokenizer(null)
                {
                    Index = -1
                }
                : new StringTokenizer(String.Substring(Index));
        }

        public void Reset()
        {
            Index = 0;
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            // empty
        }
    }
}