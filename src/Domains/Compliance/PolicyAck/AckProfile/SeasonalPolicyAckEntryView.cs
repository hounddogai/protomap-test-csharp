namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal policy ack entry.</summary>
public sealed record SeasonalPolicyAckEntryView(string CodeText, string Title, string ApprovedText, string Cost, string TenureMonthsText, string SummaryText, string Score, string Hours, string Category, string FteText, string Location, string StatusLabel)
{
    public static SeasonalPolicyAckEntryView ToSeasonalPolicyAckEntryView(SeasonalPolicyAckEntry item)
    {
        return new SeasonalPolicyAckEntryView(
            item.Code,
            item.Title,
            $"{item.Approved}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPolicyAckEntryView> ToSeasonalPolicyAckEntryViews(List<SeasonalPolicyAckEntry> items)
    {
        var views = new List<SeasonalPolicyAckEntryView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPolicyAckEntryView(item));
        }
        return views;
    }

    public static double SumSeasonalPolicyAckEntryView(SeasonalPolicyAckEntryView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedTenureMonths + parsedScore;
    }
}
