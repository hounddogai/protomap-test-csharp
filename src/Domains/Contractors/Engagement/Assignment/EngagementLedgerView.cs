namespace Acme.Workforce.Contractors.Engagement.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement ledger.</summary>
public sealed record EngagementLedgerView(string Code, string Label, string OwnerId, string Attempts, string FloorValueText, string Location, string Rating, string StatusLabel)
{
    public static EngagementLedgerView ToEngagementLedgerView(EngagementLedger item)
    {
        return new EngagementLedgerView(
            item.Code,
            item.Label,
            Text.Truncate(item.OwnerId, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementLedgerView> ToEngagementLedgerViews(List<EngagementLedger> items)
    {
        var views = new List<EngagementLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementLedgerView(item));
        }
        return views;
    }

    public static double SumEngagementLedgerView(EngagementLedgerView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedFloorValue + parsedRating;
    }
}
