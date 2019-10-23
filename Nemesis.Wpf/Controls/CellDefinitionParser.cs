using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Nemesis.Wpf.Controls
{
    internal interface ICellDefinitionParser
    {
        IEnumerable<(GridLength Length, string SharedSizeGroup, byte Count)> ParseDefinitions(string text);
    }

    internal class StandardCellDefinitionParser : ICellDefinitionParser
    {
        private StandardCellDefinitionParser() { }

        public static ICellDefinitionParser Instance { get; } = new StandardCellDefinitionParser();

        public IEnumerable<(GridLength Length, string SharedSizeGroup, byte Count)> ParseDefinitions(string text)
            => text.Split(',').Select(s => ParseLength(s));

        private static readonly Regex _beginsWithNumberPattern = new Regex(@"^(?<Number>\d+)\s*x\s*",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled
        );

        public (GridLength Length, string SharedSizeGroup, byte Count) ParseLength(in string lengthText)
        {
            if (string.IsNullOrWhiteSpace(lengthText))
                return (new GridLength(1.0, GridUnitType.Star), null, 1);

            var length = lengthText.Trim();

            string sharedSizeGroup = null;
            if (length.IndexOf("@", StringComparison.Ordinal) is var index && index > -1)
            {
                sharedSizeGroup = length.Substring(index + 1);
                length = length.Substring(0, index);
            }
            byte count = 1;
            if (_beginsWithNumberPattern.Match(length) is var m && m.Success && byte.TryParse(m.Groups["Number"]?.Value, out byte number))
            {
                count = number;
                length = _beginsWithNumberPattern.Replace(length, "");
            }


            if (string.Equals(length, "auto", StringComparison.OrdinalIgnoreCase))
                return (GridLength.Auto, sharedSizeGroup, count);

            else if (double.TryParse(length, NumberStyles.Any, CultureInfo.InvariantCulture, out var lenNumber))
                return (new GridLength(lenNumber, GridUnitType.Pixel), sharedSizeGroup, count);

            else if (length.Contains("*"))
            {
                length = length.Replace("*", "");
                if (string.IsNullOrWhiteSpace(length)) length = "1";

                if (double.TryParse(length, NumberStyles.Any, CultureInfo.InvariantCulture, out var starLenNumber))
                    return (new GridLength(starLenNumber, GridUnitType.Star), sharedSizeGroup, count);
            }
            //(\s*(?<Times>\d+)\s*x\s*)?
            throw new ArgumentOutOfRangeException(nameof(lengthText), lengthText, @"Length for column/row definitions is not supported. Possible values:
(<0-9>+x)?
[ <empty>
  auto
  (0-9)*\\*
  (0-9)+
] (@<A-z0-9>+)?");
            /*public static GridLength ParseLength(in ReadOnlySpan<char> lengthText) { 
                        var length = lengthText.Trim();
                        if (length.Length==4 && length.Contains("auto".AsSpan(), StringComparison.OrdinalIgnoreCase))
                            return new GridLength(0, GridUnitType.Auto);
                        else if (double.TryParse(length, out var lenNumber))
                            return new GridLength(lenNumber, GridUnitType.Pixel);

                        else if (length.Contains("*".AsSpan(),StringComparison.Ordinal))
                        {
                            length = length.Replace("*", "");
                            if (string.IsNullOrWhiteSpace(length)) length = "1";

                            if (double.TryParse(length, out var starLenNumber))
                                return new GridLength(starLenNumber, GridUnitType.Star);
                        }
                        throw new ArgumentOutOfRangeException(nameof(lengthText), lengthText.ToString(), @"Length for column/row definitions is not supported. Possible values:
            auto
            (0-9)+\\*
            (0-9)+");
                    }*/

        }
    }

#if NETCOREAPP3_0
    internal  static class SpanCellDefinitionParser //: ICellDefinitionParser
    {
        public static DefinitionsEnumerable ParseDefinitions([NotNull] string text) =>
            new DefinitionsEnumerable((text ?? throw new ArgumentNullException(nameof(text))).AsSpan());

        public static (GridLength Length, string SharedSizeGroup, byte Count) ParseLength(ReadOnlySpan<char> lengthText)
        {
            if (lengthText.IsEmpty || lengthText.IsWhiteSpace())
                return (new GridLength(1.0, GridUnitType.Star), null, 1);

            var length = lengthText.Trim();

            string sharedSizeGroup = null;
            if (length.IndexOf('@') is var indexAt && indexAt > -1)
            {
                sharedSizeGroup = length.Slice(indexAt + 1).ToString();
                length = length.Slice(0, indexAt).TrimEnd();
            }
            byte count = 1;
            if (length.IndexOf('x') is var indexX && indexX > 0 && SpanParser.TryParseByte(length.Slice(0, indexX), out byte number))
            {
                count = number;
                length = length.Slice(indexX + 1).TrimStart();
            }


            if (length.Length > 3 &&
                length[0] is var c0 && (c0 == 'A' || c0 == 'a') &&
                length[1] is var c1 && (c1 == 'U' || c1 == 'u') &&
                length[2] is var c2 && (c2 == 'T' || c2 == 't') &&
                length[3] is var c3 && (c3 == 'O' || c3 == 'o')
                )
                return (GridLength.Auto, sharedSizeGroup, count);

            else if (SpanParser.TryParseDouble(length, out double lenNumber))
                return (new GridLength(lenNumber, GridUnitType.Pixel), sharedSizeGroup, count);

            else if (length.IndexOf('*') is var indexStar && indexStar > -1)
            {
                var beforeStar = length.Slice(0, indexStar);

                if (beforeStar.IsEmpty || beforeStar.IsWhiteSpace())
                    return (new GridLength(1.0, GridUnitType.Star), sharedSizeGroup, count);

                if (SpanParser.TryParseDouble(beforeStar, out double starLenNumber))
                    return (new GridLength(starLenNumber, GridUnitType.Star), sharedSizeGroup, count);
            }
            //(\s*(?<Times>\d+)\s*x\s*)?
            throw new ArgumentOutOfRangeException(nameof(lengthText), lengthText.ToString(), @"Length for column/row definitions is not supported. Possible values:
(<0-9>+x)?
[ <empty>
  auto
  (0-9)*\\*
  (0-9)+
] (@<A-z0-9>+)?");
        }

        public ref struct DefinitionsEnumerable
        {
            public DefinitionsEnumerable(ReadOnlySpan<char> span, char separator = ',')
            {
                Span = span;
                Separator = separator;
            }

            private ReadOnlySpan<char> Span { get; }
            private char Separator { get; }

            public DefinitionsEnumerator GetEnumerator() => new DefinitionsEnumerator(Span, Separator);
        }

        public ref struct DefinitionsEnumerator
        {
            public DefinitionsEnumerator(ReadOnlySpan<char> span, char separator)
            {
                Span = span;
                Separator = separator;
                Current = default;

                _shouldReturnDefaultElementAndFinish = Span.IsEmpty;
            }

            private ReadOnlySpan<char> Span { get; set; }
            private char Separator { get; }

            private bool _shouldReturnDefaultElementAndFinish;

            public bool MoveNext()
            {
                if (_shouldReturnDefaultElementAndFinish)
                {
                    _shouldReturnDefaultElementAndFinish = false;
                    Current = ParseLength(Span);
                    return true;
                }

                if (Span.IsEmpty)
                {
                    Current = default;
                    Span = default;
                    return false;
                }

                int idx = Span.IndexOf(Separator);
                if (idx < 0)
                {
                    Current = ParseLength(Span);
                    Span = default;
                }
                else
                {
                    Current = ParseLength(Span.Slice(0, idx));
                    Span = Span.Slice(idx + 1);
                    if (Span.IsEmpty)
                        _shouldReturnDefaultElementAndFinish = true;
                }

                return true;
            }

            public (GridLength Length, string SharedSizeGroup, byte Count) Current { get; private set; }
        }
    }
#endif
}
