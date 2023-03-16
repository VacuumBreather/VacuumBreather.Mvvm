using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Markup;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf.MarkupExtensions;

/// <summary>A <see cref="MarkupExtension"/> adding Lorem Ipsum type sample text.</summary>
/// <seealso cref="MarkupExtensionBase"/>
/// <seealso cref="System.Windows.Markup.MarkupExtension"/>
[PublicAPI]
public class LoremIpsumExtension : MarkupExtensionBase
{
    /// <summary>Gets or sets the number of paragraphs to produce.</summary>
    public int NumParagraphs { get; set; }

    /// <summary>Gets or sets the number of sentences to produce in total or per paragraph.</summary>
    public int NumSentences { get; set; }

    /// <summary>Gets or sets the number of words to produce in total or per sentence.</summary>
    public int NumWords { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the first letter should be capitalized even if no full sentences are
    ///     generated.
    /// </summary>
    public bool UppercaseFirstLetter { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether punctuation should be used even if no full sentences are generated.</summary>
    public bool UsePunctuation { get; set; }

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (NumParagraphs > 1)
        {
            return LoremIpsum.Paragraphs(NumWords, NumSentences, NumParagraphs);
        }

        if (NumParagraphs == 1)
        {
            return LoremIpsum.Paragraph(NumWords, NumSentences);
        }

        return NumSentences > 0
                   ? LoremIpsum.Sentence(NumWords)
                   : LoremIpsum.Words(NumWords, UppercaseFirstLetter, UsePunctuation);
    }

    private static class LoremIpsum
    {
        internal static string Paragraph(int wordCount, int sentenceCount)
        {
            return string.Join(separator: " ",
                               Enumerable.Range(start: 0, sentenceCount).Select(_ => Sentence(wordCount)));
        }

        internal static string Paragraphs(int wordCount, int sentenceCount, int paragraphCount)
        {
            return string.Join(Environment.NewLine + Environment.NewLine,
                               Enumerable.Range(start: 0, paragraphCount)
                                         .Select(_ => Paragraph(wordCount, sentenceCount))
                                         .ToArray());
        }

        internal static string Sentence(int wordCount)
        {
            return $"{Words(wordCount, uppercaseFirstLetter: true, includePunctuation: true)}."
                   .Replace(oldValue: ",.", newValue: ".", StringComparison.Ordinal)
                   .Remove(pattern: "..");
        }

        internal static string Words(int wordCount, bool uppercaseFirstLetter = true, bool includePunctuation = false)
        {
            var source = string.Join(separator: " ", Source.WordList(includePunctuation).Take(wordCount));

            if (uppercaseFirstLetter)
            {
                source = source.UppercaseFirst();
            }

            return source;
        }
    }

    [SuppressMessage(category: "ReSharper", checkId: "StringLiteralTypo", Justification = "It is lorem ipsum.")]
    private static class Source
    {
        private static readonly Random Rng = new();

        private static string Text { get; set; } =
            @"lorem ipsum amet, pellentesque mattis accumsan maximus etiam mollis ligula non iaculis ornare mauris efficitur ex eu rhoncus aliquam in hac habitasse platea dictumst maecenas ultrices, purus at venenatis auctor, sem nulla urna, molestie nisi mi a ut euismod nibh id libero lacinia, sit amet lacinia lectus viverra donec scelerisque dictum enim, dignissim dolor cursus morbi rhoncus, elementum magna sed, sed velit consectetur adipiscing elit curabitur nulla, eleifend vel, tempor metus phasellus vel pulvinar, lobortis quis, nullam felis orci congue vitae augue nisi, tincidunt id, posuere fermentum facilisis ultricies mi, nisl fusce neque, vulputate integer tortor tempus praesent proin quis nunc massa congue, quam auctor eros placerat eros, leo nec, sapien egestas duis feugiat, vestibulum porttitor, odio sollicitudin arcu, et aenean sagittis ante urna fringilla, risus et, vivamus semper nibh, eget finibus est laoreet justo commodo sagittis, vitae, nunc, diam ac, tellus posuere, condimentum enim tellus, faucibus suscipit ac nec turpis interdum malesuada fames primis quisque pretium ex, feugiat porttitor massa, vehicula dapibus blandit, hendrerit elit, aliquet nam orci, fringilla blandit ullamcorper mauris, ultrices consequat tempor, convallis gravida sodales volutpat finibus, neque pulvinar varius, porta laoreet, eu, ligula, porta, placerat, lacus pharetra erat bibendum leo, tristique cras rutrum at, dui tortor, in, varius arcu interdum, vestibulum, magna, ante, imperdiet erat, luctus odio, non, dui, volutpat, bibendum, quam, euismod, mattis, class aptent taciti sociosqu ad litora torquent per conubia nostra, inceptos himenaeos suspendisse lorem, a, sem, eleifend, commodo, dolor, cursus, luctus, lectus,";

        internal static IEnumerable<string> WordList(bool includePunctuation)
        {
            return includePunctuation ? Rearrange(Text) : Rearrange(Text.Remove(pattern: ","));
        }

        [SuppressMessage(category: "Security",
                         checkId: "SCS0005:Weak random number generator.",
                         Justification = "Not security relevant.")]
        [SuppressMessage(category: "Security",
                         checkId: "CA5394:Do not use insecure randomness",
                         Justification = "Not security relevant.")]
        private static IEnumerable<string> Rearrange(string words)
        {
            return words.Split(separator: " ").OrderBy(_ => Rng.Next());
        }
    }
}