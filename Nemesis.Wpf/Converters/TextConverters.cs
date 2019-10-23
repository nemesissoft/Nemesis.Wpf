using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;
using Nemesis.Wpf.Converters;
// ReSharper disable StringLiteralTypo

namespace Nemesis.Wpf.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public sealed class PluralizeConverter : BaseConverter
    {
        public PluralizeConverter() : this("None") { }

        public PluralizeConverter(string noneText) => NoneText = noneText;

        [ConstructorArgument("noneText")]
        public string NoneText { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            static bool IsNumeric(object obj)
                 => obj is double || obj is float ||
                    obj is int || obj is uint ||
                    obj is short || obj is ushort ||
                    obj is byte || obj is sbyte ||
                    obj is long || obj is ulong ||
                    obj is decimal;

            if (parameter is string noun && value != null && IsNumeric(value) && System.Convert.ToInt32(value) is int number)
            {
                if (number <= 0) return NoneText;

                else if (number == 1) return $"{number} {noun}";
                else return $"{number} {Pluralize(noun)}";
            }
            return Binding.DoNothing;
        }

        private static string Pluralize(string noun)
        {
            string RegularCase() => noun.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? $"{noun}es" : $"{noun}s";

            if (string.IsNullOrWhiteSpace(noun)) return noun;

            if (_uncountable.Contains(noun)) return noun;

            return noun.ToLowerInvariant() switch
            {
                // Pronouns.
                "i" => "we",
                "me" => "us",
                "he" => "they",
                "she" => "they",
                "them" => "them",
                "myself" => "ourselves",
                "yourself" => "yourselves",
                "itself" => "themselves",
                "herself" => "themselves",
                "himself" => "themselves",
                "themself" => "themselves",
                "is" => "are",
                "was" => "were",
                "has" => "have",
                "this" => "these",
                "that" => "those",
                // Words ending in with a consonant and `o`.
                "echo" => "echoes",
                "dingo" => "dingoes",
                "volcano" => "volcanoes",
                "tornado" => "tornadoes",
                "torpedo" => "torpedoes",
                // Ends with `us`.
                "genus" => "genera",
                "viscus" => "viscera",
                // Ends with `ma`.
                "stigma" => "stigmata",
                "stoma" => "stomata",
                "dogma" => "dogmata",
                "lemma" => "lemmata",
                "schema" => "schemata",
                "anathema" => "anathemata",
                // Other irregular rules.
                "ox" => "oxen",
                "axe" => "axes",
                "die" => "dice",
                "yes" => "yeses",
                "foot" => "feet",
                "eave" => "eaves",
                "goose" => "geese",
                "tooth" => "teeth",
                "quiz" => "quizzes",
                "human" => "humans",
                "man" => "men",
                "woman" => "women",
                "child" => "children",
                "proof" => "proofs",
                "carve" => "carves",
                "valve" => "valves",
                "looey" => "looies",
                "thief" => "thieves",
                "groove" => "grooves",
                "pickaxe" => "pickaxes",
                "passerby" => "passersby",
                "cookie" => "cookies",
                "whiskey" => "whiskies",
                _ => RegularCase()
            };
        }


        // Singular words with no plurals
        private static readonly HashSet<string> _uncountable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "adulthood", "advice", "agenda", "aid", "aircraft", "alcohol", "ammo", "anime", "athletics", "audio",
            "bison", "blood", "bream","buffalo","butter","carp","cash","chassis","chess","clothing","cod","commerce",
            "cooperation","corps","debris","diabetes","digestion","elk","energy","equipment","excretion","expertise",
            "firmware","flounder","fun","gallows","garbage","graffiti","headquarters","health","herpes","highjinks",
            "homework","housework","information","jeans","justice","kudos","labour","literature","machinery","mackerel",
            "mail","media","mews","moose","music","mud","manga","news","only","pike","plankton","pliers","police","pollution",
            "premises","rain","research","rice","salmon","scissors","series","sewage","shambles","shrimp","species","staff",
            "swine","tennis","traffic","transportation","trout","tuna","wealth","welfare","whiting","wildebeest","wildlife","you"
        };
    }

    [ValueConversion(typeof(string), typeof(string))]
    public sealed class StringToOneLineConverter : BaseConverter
    {
        private const string ENTER = "⏎";
        private const string TAB = "↦";

        [Pure]
        public static string ReplaceControlChars(string t) => t?.Replace("\t", TAB).Replace("\r\n", ENTER).Replace("\n", ENTER).Replace("\r", ENTER) ?? "";

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string text ? ReplaceControlChars(text)
                : (value is IEnumerable<string> lines ? lines.Select(ReplaceControlChars).ToList() : Binding.DoNothing);
    }

    [ValueConversion(typeof(object), typeof(string))]
    public sealed class EmptyStringConverter : BaseConverter
    {
        public EmptyStringConverter() : this("∅") { }

        public EmptyStringConverter(string emptyText) => EmptyText = emptyText;

        [ConstructorArgument("emptyText")]
        public string EmptyText { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is null || value is string text && string.IsNullOrEmpty(text)
                ? EmptyText
                : value;
    }

    /// <summary>
    ///     Converts a String into a Visibility enumeration (and back)
    ///     The FalseEquivalent can be declared with the "FalseEquivalent" property
    /// </summary>
    [ValueConversion(typeof(string), typeof(Visibility))]
    [MarkupExtensionReturnType(typeof(StringToVisibilityConverter))]
    public class StringToVisibilityConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        ///     Initialize the properties with standard values
        /// </summary>
        public StringToVisibilityConverter()
        {
            FalseEquivalent = Visibility.Collapsed;
            OppositeStringValue = false;
        }

        /// <summary>
        ///     FalseEquivalent (default : Visibility.Collapsed => see Constructor)
        /// </summary>
        public Visibility FalseEquivalent { get; set; }

        /// <summary>
        ///     Define whether the opposite boolean value is crucial (default : false)
        /// </summary>
        public bool OppositeStringValue { get; set; }

        #region MarkupExtension "overrides"

        public override object ProvideValue(IServiceProvider serviceProvider) => new StringToVisibilityConverter
        {
            FalseEquivalent = FalseEquivalent,
            OppositeStringValue = OppositeStringValue
        };

        #endregion

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value == null || value is string) && targetType == typeof(Visibility))
            {
                return OppositeStringValue
                    ? (string.IsNullOrEmpty((string)value) ? Visibility.Visible : FalseEquivalent)
                    : (string.IsNullOrEmpty((string)value) ? FalseEquivalent : Visibility.Visible);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return OppositeStringValue
                    ? (visibility == Visibility.Visible ? string.Empty : "visible")
                    : (visibility == Visibility.Visible ? "visible" : string.Empty);
            }
            return value;
        }

        #endregion
    }
}
