namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using System.Globalization;

/// <summary>How screens and exports show a strict reorg estimate.</summary>
public sealed record StrictReorgEstimateView(string Code, string NameText, string FloorValue, string Score, string Attempts, string StatusLabel)
{
    public static StrictReorgEstimateView ToStrictReorgEstimateView(StrictReorgEstimate item)
    {
        return new StrictReorgEstimateView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictReorgEstimateView> ToStrictReorgEstimateViews(List<StrictReorgEstimate> items)
    {
        var views = new List<StrictReorgEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToStrictReorgEstimateView(item));
        }
        return views;
    }

    public static double SumStrictReorgEstimateView(StrictReorgEstimateView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedScore + (double)parsedAttempts;
    }
}
