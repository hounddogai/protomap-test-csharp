namespace Acme.Workforce.Compensation.MeritCycle.Target;

using System.Globalization;

/// <summary>How screens and exports show a primary merit cycle window.</summary>
public sealed record PrimaryMeritCycleWindowView(string Code, string NameText, string Pinned, string VersionText, string Threshold, string CostText, string Archived, string EndsAt, string GraceDays, string Approved, string Notes, string StatusLabel)
{
    public static PrimaryMeritCycleWindowView ToPrimaryMeritCycleWindowView(PrimaryMeritCycleWindow item)
    {
        return new PrimaryMeritCycleWindowView(
            item.Code,
            item.Name,
            $"{item.Pinned}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryMeritCycleWindowView> ToPrimaryMeritCycleWindowViews(List<PrimaryMeritCycleWindow> items)
    {
        var views = new List<PrimaryMeritCycleWindowView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryMeritCycleWindowView(item));
        }
        return views;
    }

    public static double SumPrimaryMeritCycleWindowView(PrimaryMeritCycleWindowView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedThreshold + parsedCost;
    }
}
