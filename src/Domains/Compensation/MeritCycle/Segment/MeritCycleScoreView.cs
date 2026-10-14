namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle score.</summary>
public sealed record MeritCycleScoreView(string Code, string Name, string Margin, string UpdatedAt, string JobCode, string StatusLabel)
{
    public static MeritCycleScoreView ToMeritCycleScoreView(MeritCycleScore item)
    {
        return new MeritCycleScoreView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleScoreView> ToMeritCycleScoreViews(List<MeritCycleScore> items)
    {
        var views = new List<MeritCycleScoreView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleScoreView(item));
        }
        return views;
    }

    public static double SumMeritCycleScoreView(MeritCycleScoreView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedUpdatedAt;
    }
}
