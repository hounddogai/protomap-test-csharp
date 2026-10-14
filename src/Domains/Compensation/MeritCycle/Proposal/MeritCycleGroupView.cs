namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a merit cycle group.</summary>
public sealed record MeritCycleGroupView(string CodeText, string Name, string Category, string BadgeText, string Unit, string Score, string StatusLabel)
{
    public static MeritCycleGroupView ToMeritCycleGroupView(MeritCycleGroup item)
    {
        return new MeritCycleGroupView(
            item.Code,
            item.Name,
            item.Category,
            item.Badge,
            item.Unit,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleGroupView> ToMeritCycleGroupViews(List<MeritCycleGroup> items)
    {
        var views = new List<MeritCycleGroupView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleGroupView(item));
        }
        return views;
    }

    public static double SumMeritCycleGroupView(MeritCycleGroupView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
