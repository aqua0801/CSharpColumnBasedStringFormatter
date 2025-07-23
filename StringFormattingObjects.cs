using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringFormatting
{
    public class StringFormattingObjects
    {
        internal static class StringFormattingAlgorithm
        {
            public static string PadStringToTextRendererWidth(string TextToBePadded, string PaddingText, float TargetWidth, Font RenderFont, StringFormatter.PadAlign PadAlign)
            {
                if (PadAlign == StringFormatter.PadAlign.Left)
                    return _PadStringToTextRendererWidth(TextToBePadded, PaddingText, TargetWidth, RenderFont, false, true);
                else if (PadAlign == StringFormatter.PadAlign.Right)
                    return _PadStringToTextRendererWidth(TextToBePadded, PaddingText, TargetWidth, RenderFont, false, false);
                else
                    return _PadStringToTextRendererWidth(TextToBePadded, PaddingText, TargetWidth, RenderFont, true, false);
            }

            private static string _PadStringToTextRendererWidth(string TextToBePadded, string PaddingText, float TargetWidth, Font RenderFont, bool CenterPadding = true, bool PadLeft = true)
            {
                var FirstMeasure = TextRenderer.MeasureText(TextToBePadded, RenderFont).Width;
                if (FirstMeasure > TargetWidth) return TextToBePadded;

                StringBuilder PaddingBuilder = new StringBuilder();
                PaddingBuilder.Append(TextToBePadded);
                PaddingBuilder.Append(PaddingText);

                while (TextRenderer.MeasureText(PaddingBuilder.ToString(), RenderFont).Width < TargetWidth)
                {
                    if (PadLeft)
                    {
                        PaddingBuilder.Insert(0, PaddingText);
                    }
                    else
                    {
                        PaddingBuilder.Append(PaddingText);
                    }
                    if (CenterPadding) PadLeft = !PadLeft;
                }
                return PaddingBuilder.ToString();
            }

            public static string ToFormatRegexKey(string text)
            {
                var sb = new StringBuilder();
                foreach (char c in text)
                {
                    if (c >= 0x4e00 && c <= 0x9fff)
                        sb.Append("C");
                    else if (char.IsLetter(c))
                        sb.Append("E");
                    else if (char.IsDigit(c))
                        sb.Append("N");
                    else if (c == '.')
                        sb.Append(".");
                    else if ("~-=.? \u00A0".Contains(c))
                        sb.Append(c);
                    else
                        sb.Append("S");
                }
                return sb.ToString();
            }


            public static float ToFixedWidth(string text)
            {
                float sum = 0f;
                foreach (char c in text)
                {
                    if (c >= 0x4e00 && c <= 0x9fff)
                        sum += 2f;
                    else if (char.IsLetter(c))
                        sum += 1f;
                    else if (char.IsDigit(c))
                        sum += 1f;
                    else if (c == '.')
                        sum += 1f;
                    else if ("~-=.? \u00A0".Contains(c))
                        sum += 1f;
                    else
                        sum += 1f;
                }
                return sum;
            }

            public static string PadToFixedWidth(string text, string padding, float width, StringFormatter.PadAlign align)
            {
                float currentWidth = ToFixedWidth(text);
                if (currentWidth >= width) return text;
                int paddingCount = (int)Math.Ceiling((width - currentWidth) / ToFixedWidth(padding));
                if (align == StringFormatter.PadAlign.Left)
                    return string.Concat(Enumerable.Repeat(padding, paddingCount)) + text;
                else if (align == StringFormatter.PadAlign.Right)
                    return text + string.Concat(Enumerable.Repeat(padding, paddingCount));
                else
                {
                    int leftPad = paddingCount / 2;
                    int rightPad = paddingCount - leftPad;
                    return string.Concat(Enumerable.Repeat(padding, leftPad)) + text + string.Concat(Enumerable.Repeat(padding, rightPad));
                }
            }

            public static string RegexStringPadding(
                string TextToBePadded,
                string PaddingText,
                float TargetWidth,
                Graphics g,
                Font RenderFont,
                Dictionary<string, int> PaddingMap,
                StringFormatter.PadAlign PadAlign)
            {
                string formatKey = ToFormatRegexKey(TextToBePadded) + "|" + PaddingText + "|" + TargetWidth.ToString("F2");
                int requiredPadding;

                if (!PaddingMap.TryGetValue(formatKey, out requiredPadding))
                {
                    var ToBePaddedWidth = g.MeasureString(TextToBePadded, RenderFont, int.MaxValue, StringFormat.GenericTypographic).Width;
                    if (ToBePaddedWidth >= TargetWidth) return TextToBePadded;

                    float PaddingTextWidth;
                    if (String.IsNullOrWhiteSpace(PaddingText))
                    {
                        PaddingText = "\u00A0";
                        PaddingTextWidth = g.MeasureString(PaddingText, RenderFont, int.MaxValue, StringFormat.GenericTypographic).Width;
                        PaddingText = " ";
                    }
                    else
                    {
                        PaddingTextWidth = g.MeasureString(PaddingText, RenderFont, int.MaxValue, StringFormat.GenericTypographic).Width;
                    }

                    requiredPadding = (int)Math.Round((TargetWidth - ToBePaddedWidth) / PaddingTextWidth, MidpointRounding.AwayFromZero);
                    PaddingMap[formatKey] = requiredPadding;
                }
                if (PadAlign == StringFormatter.PadAlign.Left)
                    return string.Concat(Enumerable.Repeat(PaddingText, requiredPadding)) + TextToBePadded;
                else if (PadAlign == StringFormatter.PadAlign.Right)
                    return TextToBePadded + string.Concat(Enumerable.Repeat(PaddingText, requiredPadding));
                else
                {
                    int LeftPad = requiredPadding / 2;
                    int RightPad = requiredPadding - LeftPad;
                    return string.Concat(Enumerable.Repeat(PaddingText, LeftPad)) + TextToBePadded + string.Concat(Enumerable.Repeat(PaddingText, RightPad));
                }
            }

            public static (Bitmap, Graphics) CreateGraphics()
            {
                using (var bitmap = new Bitmap(1, 1))
                {
                    var g = Graphics.FromImage(bitmap);
                    return (bitmap, g);
                }
            }

        }



        public class StringFormatter : IDisposable
        {
            public enum PadAlign
            {
                Left = 0,
                Center = 1,
                Right = 2
            }

            public enum FormattingAlgorithm
            {
                RenderSize = 0,
                RenderSizeRegex = 1,
                Fixed = 2
            }

            public enum Separator
            {
                Major, Minor
            }

            private const string PadString = " ";

            private Graphics g;
            private Bitmap b;
            private Dictionary<string, float> RegexWidthMap;
            private Dictionary<string, int> RegexPaddingMap;

            private float PaddingWidth;

            private Font RenderFont;
            private FormattingAlgorithm FormatAlgorithm;

            private List<string> StringTemps;

            private StringBuilder FormatBuilder;

            private List<float> MaxColumnWidths;
            /// <summary>
            /// Width Calculating Algorithm
            /// Param : TextToBeCalculated
            /// Return : Width
            /// </summary>
            private Func<string, float> CalculateWidthAlgorithm;
            /// <summary>
            /// Padding algorithm
            /// Param : TextToBePadded , PaddingString , TargetWidth
            /// Return : Padded string 
            /// </summary>
            private Func<string, string, float, string> PadStringAlgorithm;

            private Dictionary<int, Separator> SeparatorMap;

            public int PaddingSpace = 2;

            public StringFormatter(FormattingAlgorithm Algorithm, PadAlign Align, int Capacity, Font RenderFont = null)
            {
                this.FormatAlgorithm = Algorithm;
                this.StringTemps = new List<string>(Capacity);
                this.FormatBuilder = new StringBuilder(Capacity);
                this.MaxColumnWidths = new List<float>(Capacity);
                this.SeparatorMap = new Dictionary<int, Separator>();

                if (this.FormatAlgorithm == FormattingAlgorithm.RenderSize || this.FormatAlgorithm == FormattingAlgorithm.RenderSizeRegex)
                {
                    (this.b, this.g) = StringFormattingAlgorithm.CreateGraphics();
                    this.RenderFont = (RenderFont == null) ? new Font("Calibri", 9) : RenderFont;
                    this.PaddingWidth = this.g.MeasureString("\u00A0", this.RenderFont, int.MaxValue, StringFormat.GenericTypographic).Width;
                }

                if (this.FormatAlgorithm == FormattingAlgorithm.RenderSize)
                {
                    this.CalculateWidthAlgorithm = (t) =>
                    {
                        return this.g.MeasureString(t, this.RenderFont, int.MaxValue, StringFormat.GenericTypographic).Width + this.PaddingWidth * this.PaddingSpace;
                    };
                    this.PadStringAlgorithm = (t, p, w) =>
                    {
                        return StringFormattingAlgorithm.PadStringToTextRendererWidth(t, p, w, this.RenderFont, Align);
                    };
                }
                else if (this.FormatAlgorithm == FormattingAlgorithm.RenderSizeRegex)
                {
                    this.RegexWidthMap = new Dictionary<string, float>();
                    this.RegexPaddingMap = new Dictionary<string, int>();
                    this.CalculateWidthAlgorithm = (t) =>
                    {
                        string Regex = StringFormattingAlgorithm.ToFormatRegexKey(t);
                        if (this.RegexWidthMap.TryGetValue(t, out float Width))
                            return Width;
                        Width = this.g.MeasureString(t, this.RenderFont, int.MaxValue, StringFormat.GenericTypographic).Width + this.PaddingWidth * this.PaddingSpace;
                        this.RegexWidthMap[Regex] = Width;
                        return Width;
                    };
                    this.PadStringAlgorithm = (t, p, w) =>
                    {
                        return StringFormattingAlgorithm.RegexStringPadding(t, p, w, this.g, this.RenderFont, this.RegexPaddingMap, Align);
                    };
                }
                else if (this.FormatAlgorithm == FormattingAlgorithm.Fixed)
                {
                    this.CalculateWidthAlgorithm = (t) => StringFormattingAlgorithm.ToFixedWidth(t) + this.PaddingSpace;
                    this.PadStringAlgorithm = (t, p, w) => StringFormattingAlgorithm.PadToFixedWidth(t, p, w, Align);
                }
            }


            /// <summary>
            /// Clear object (not Dispose , reuseable , but not recommended as this class is intended to be short lifecycle , disposed when finished)
            /// </summary>
            public void Clear()
            {
                if (this.FormatAlgorithm == FormattingAlgorithm.RenderSize)
                {
                    this.b?.Dispose();
                    this.g?.Dispose();
                    (this.b, this.g) = StringFormattingAlgorithm.CreateGraphics();
                }
                else if (this.FormatAlgorithm == FormattingAlgorithm.RenderSizeRegex)
                {
                    this.b?.Dispose();
                    this.g?.Dispose();
                    (this.b, this.g) = StringFormattingAlgorithm.CreateGraphics();
                    this.RegexWidthMap.Clear();
                    this.RegexPaddingMap.Clear();
                }
                else if (this.FormatAlgorithm == FormattingAlgorithm.Fixed)
                {
                    //No specific action for fixed algorithm.
                }
                this.StringTemps.Clear();
                this.FormatBuilder.Clear();
            }

            /// <summary>
            /// Insert separator to index(end of the string temp by default) (not pre-newlining , instead , newlined after separator)
            /// </summary>
            public void AddSeparator(StringFormatter.Separator sep, int? index = null)
            {
                if (index == null)
                {
                    this.SeparatorMap.Add(this.StringTemps.Count, sep);
                    this.StringTemps.Add("");
                }
                else
                {
                    this.SeparatorMap.Add(index.Value, sep);
                    this.StringTemps.Insert(index.Value, "");
                }
            }

            /// <summary>
            /// Get current string temp count.
            /// </summary>
            public int GetStringTempsCount() => this.StringTemps.Count;

            /// <summary>
            /// Add string (Add Width list if ColumnIndex exceeds , therefore Index is required to be asc. if Random Access is required , modify this method !)
            /// </summary>
            public void AddStringTemps(string TextToAdd, int ColumnIndex)
            {
                float Width = this.CalculateWidthAlgorithm(TextToAdd);
                if (ColumnIndex >= this.MaxColumnWidths.Count)
                    this.MaxColumnWidths.Add(Width);
                else
                {
                    if (Width > this.MaxColumnWidths[ColumnIndex])
                        this.MaxColumnWidths[ColumnIndex] = Width;
                }
                this.StringTemps.Add(TextToAdd);
            }

            /// <summary>
            /// Add string without considering width.
            /// </summary>
            public void AddStringTemps(string TextToAdd)
            {
                this.StringTemps.Add(TextToAdd);
            }

            /// <summary>
            /// Newline , equal to .AddString(Environment.NewLine) but adds more readability.
            /// </summary>
            public void NewLine()
            {
                this.StringTemps.Add(Environment.NewLine);
            }

            /// <summary>
            /// Create respective separator string.
            /// </summary>
            public string GenerateSeparators(Separator sep)
            {
                StringBuilder SepBuilder = new StringBuilder(1000);
                if (sep == Separator.Major)
                    foreach (var width in this.MaxColumnWidths)
                        SepBuilder.Append(this.PadStringAlgorithm("=", "=", width));
                else if (sep == Separator.Minor)
                    foreach (var width in this.MaxColumnWidths)
                        SepBuilder.Append(this.PadStringAlgorithm("-", "-", width));

                return SepBuilder.ToString();
            }

            /// <summary>
            /// Generate formatted string with optional callback function for progress monitoring.
            /// </summary>
            public string ToFormattedString(int interval = int.MaxValue, Action<int> CallBack = null)
            {
                string MajorSep = this.GenerateSeparators(Separator.Major);
                string MinorSep = this.GenerateSeparators(Separator.Minor);

                int ColumnIndex = 0;
                for (int index = 0; index < this.StringTemps.Count; index++)
                {
                    string StringTemp = this.StringTemps[index];

                    if (this.SeparatorMap.TryGetValue(index, out Separator sep))
                    {
                        if (sep == Separator.Major)
                            this.FormatBuilder.Append(MajorSep);
                        else if (sep == Separator.Minor)
                            this.FormatBuilder.Append(MinorSep);
                        this.FormatBuilder.AppendLine();
                        ColumnIndex = 0;
                    }
                    else
                    {
                        if (StringTemp == Environment.NewLine)
                        {
                            ColumnIndex = 0;
                            this.FormatBuilder.AppendLine();
                        }

                        else
                        {
                            float Width = (ColumnIndex < this.MaxColumnWidths.Count) ? this.MaxColumnWidths[ColumnIndex] : 0f;
                            this.FormatBuilder.Append(this.PadStringAlgorithm(StringTemp, StringFormatter.PadString, Width));
                            ColumnIndex++;
                        }
                    }

                    if (CallBack != null && index % interval == 0)
                        CallBack.Invoke(index);
                }

                return this.FormatBuilder.ToString();
            }

            private bool _disposed = false;

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this._disposed)
                    return;

                if (disposing)
                {
                    this.g?.Dispose();
                    this.b?.Dispose();
                    this.RegexWidthMap?.Clear();
                    this.RegexWidthMap = null;
                    this.RegexPaddingMap?.Clear();
                    this.RegexPaddingMap = null;
                    this.StringTemps?.Clear();
                    this.StringTemps = null;
                    this.FormatBuilder?.Clear();
                    this.FormatBuilder = null;
                    this.SeparatorMap?.Clear();
                    this.SeparatorMap = null;
                }

                this._disposed = true;
            }

        }
    }
}
