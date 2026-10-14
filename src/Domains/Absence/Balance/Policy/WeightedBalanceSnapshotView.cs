namespace Acme.Workforce.Absence.Balance.Policy;

using System.Globalization;

/// <summary>How screens and exports show a weighted balance snapshot.</summary>
public sealed record WeightedBalanceSnapshotView(string Code, string Label, string StartsAtText, string NameText, string JobCode, string SummaryText, string StatusLabel)
{
    public static WeightedBalanceSnapshotView ToWeightedBalanceSnapshotView(WeightedBalanceSnapshot item)
    {
        return new WeightedBalanceSnapshotView(
            item.Code,
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.JobCode,
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedBalanceSnapshotView> ToWeightedBalanceSnapshotViews(List<WeightedBalanceSnapshot> items)
    {
        var views = new List<WeightedBalanceSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedBalanceSnapshotView(item));
        }
        return views;
    }

    public static double SumWeightedBalanceSnapshotView(WeightedBalanceSnapshotView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
