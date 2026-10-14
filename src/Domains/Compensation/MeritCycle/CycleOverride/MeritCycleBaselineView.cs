namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using System.Globalization;

/// <summary>How screens and exports show a merit cycle baseline.</summary>
public sealed record MeritCycleBaselineView(string CodeText, string NameText, string UpdatedAt, string Maximum, string Attempts, string StatusLabel)
{
    public static MeritCycleBaselineView ToMeritCycleBaselineView(MeritCycleBaseline item)
    {
        return new MeritCycleBaselineView(
            item.Code,
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleBaselineView> ToMeritCycleBaselineViews(List<MeritCycleBaseline> items)
    {
        var views = new List<MeritCycleBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleBaselineView(item));
        }
        return views;
    }

    public static double SumMeritCycleBaselineView(MeritCycleBaselineView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedMaximum + (double)parsedAttempts;
    }
}
