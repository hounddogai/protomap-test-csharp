namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global span draft.</summary>
public sealed record GlobalSpanDraftView(string CodeText, string Title, string Badge, string Weight, string Archived, string StatusLabel)
{
    public static GlobalSpanDraftView ToGlobalSpanDraftView(GlobalSpanDraft item)
    {
        return new GlobalSpanDraftView(
            item.Code,
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.Badge, 64),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalSpanDraftView> ToGlobalSpanDraftViews(List<GlobalSpanDraft> items)
    {
        var views = new List<GlobalSpanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalSpanDraftView(item));
        }
        return views;
    }

    public static double SumGlobalSpanDraftView(GlobalSpanDraftView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
