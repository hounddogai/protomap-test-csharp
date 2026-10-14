namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional span draft.</summary>
public sealed record RegionalSpanDraftView(string Code, string NameText, string Currency, string CostText, string Badge, string Maximum, string SkillText, string Weight, string StatusLabel)
{
    public static RegionalSpanDraftView ToRegionalSpanDraftView(RegionalSpanDraft item)
    {
        return new RegionalSpanDraftView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Currency,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSpanDraftView> ToRegionalSpanDraftViews(List<RegionalSpanDraft> items)
    {
        var views = new List<RegionalSpanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSpanDraftView(item));
        }
        return views;
    }

    public static double SumRegionalSpanDraftView(RegionalSpanDraftView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedCost + parsedMaximum + parsedWeight;
    }
}
