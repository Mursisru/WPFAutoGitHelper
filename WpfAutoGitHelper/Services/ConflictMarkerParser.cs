using System;
using System.Collections.Generic;
using System.Text;

namespace WpfAutoGitHelper.Services
{
    public sealed class ConflictHunk
    {
        public int StartLine { get; set; }
        public string Ours { get; set; } = "";
        public string Theirs { get; set; } = "";
        public string Separator { get; set; } = "";
    }

    public static class ConflictMarkerParser
    {
        public static IList<ConflictHunk> Parse(string text)
        {
            var hunks = new List<ConflictHunk>();
            if (string.IsNullOrEmpty(text))
                return hunks;

            var lines = text.Replace("\r\n", "\n").Split('\n');
            ConflictHunk current = null;
            var section = 0;
            var ours = new StringBuilder();
            var theirs = new StringBuilder();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.StartsWith("<<<<<<<", StringComparison.Ordinal))
                {
                    current = new ConflictHunk { StartLine = i };
                    section = 1;
                    ours.Clear();
                    theirs.Clear();
                    continue;
                }

                if (line.StartsWith("=======", StringComparison.Ordinal) && section == 1)
                {
                    section = 2;
                    continue;
                }

                if (line.StartsWith(">>>>>>>", StringComparison.Ordinal) && section == 2 && current != null)
                {
                    current.Ours = ours.ToString();
                    current.Theirs = theirs.ToString();
                    current.Separator = line;
                    hunks.Add(current);
                    current = null;
                    section = 0;
                    continue;
                }

                if (section == 1)
                    ours.AppendLine(line);
                else if (section == 2)
                    theirs.AppendLine(line);
            }

            return hunks;
        }

        public static string ApplyResolution(string original, IList<ConflictHunk> hunks, ConflictResolutionChoice choice)
        {
            if (string.IsNullOrEmpty(original) || hunks == null || hunks.Count == 0)
                return original;

            var lines = new List<string>(original.Replace("\r\n", "\n").Split('\n'));
            for (var h = hunks.Count - 1; h >= 0; h--)
            {
                var conflict = hunks[h];
                var start = FindMarkerLine(lines, "<<<<<<<", conflict.StartLine);
                if (start < 0)
                    continue;

                var mid = FindMarkerLine(lines, "=======", start);
                var end = FindMarkerLine(lines, ">>>>>>>", mid < 0 ? start : mid);
                if (mid < 0 || end < 0)
                    continue;

                var replacement = new List<string>();
                switch (choice)
                {
                    case ConflictResolutionChoice.Ours:
                        replacement.AddRange(conflict.Ours.TrimEnd().Split('\n'));
                        break;
                    case ConflictResolutionChoice.Theirs:
                        replacement.AddRange(conflict.Theirs.TrimEnd().Split('\n'));
                        break;
                    case ConflictResolutionChoice.Both:
                        replacement.AddRange(conflict.Ours.TrimEnd().Split('\n'));
                        if (replacement.Count > 0 && !string.IsNullOrEmpty(replacement[replacement.Count - 1]))
                            replacement.Add("");
                        replacement.AddRange(conflict.Theirs.TrimEnd().Split('\n'));
                        break;
                }

                lines.RemoveRange(start, end - start + 1);
                lines.InsertRange(start, replacement);
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static int FindMarkerLine(List<string> lines, string prefix, int hint)
        {
            for (var i = Math.Max(0, hint); i < lines.Count; i++)
            {
                if (lines[i].StartsWith(prefix, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }
    }

    public enum ConflictResolutionChoice
    {
        Ours,
        Theirs,
        Both,
    }
}
