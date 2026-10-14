namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using System.Globalization;

/// <summary>How screens and exports show a tiered merit cycle checkpoint.</summary>
public sealed record TieredMeritCycleCheckpointView(string Code, string Title, string Amount, string CeilingText, string ExpiresAt, string StatusLabel)
{
    public static TieredMeritCycleCheckpointView ToTieredMeritCycleCheckpointView(TieredMeritCycleCheckpoint item)
    {
        return new TieredMeritCycleCheckpointView(
            item.Code,
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredMeritCycleCheckpointView> ToTieredMeritCycleCheckpointViews(List<TieredMeritCycleCheckpoint> items)
    {
        var views = new List<TieredMeritCycleCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToTieredMeritCycleCheckpointView(item));
        }
        return views;
    }

    public static double SumTieredMeritCycleCheckpointView(TieredMeritCycleCheckpointView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedAmount + parsedCeiling + (double)parsedExpiresAt;
    }
}
