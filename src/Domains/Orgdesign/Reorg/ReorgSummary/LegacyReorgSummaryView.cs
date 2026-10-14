namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy reorg summary.</summary>
public sealed record LegacyReorgSummaryView(string Code, string Title, string Badge, string AmountText, string Archived, string ChannelText, string StatusLabel)
{
    public static LegacyReorgSummaryView ToLegacyReorgSummaryView(LegacyReorgSummary item)
    {
        return new LegacyReorgSummaryView(
            item.Code,
            item.Title,
            item.Badge,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Channel, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyReorgSummaryView> ToLegacyReorgSummaryViews(List<LegacyReorgSummary> items)
    {
        var views = new List<LegacyReorgSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyReorgSummaryView(item));
        }
        return views;
    }

    public static double SumLegacyReorgSummaryView(LegacyReorgSummaryView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
