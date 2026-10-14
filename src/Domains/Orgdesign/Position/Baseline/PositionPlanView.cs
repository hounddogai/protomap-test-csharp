namespace Acme.Workforce.Orgdesign.Position.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position plan.</summary>
public sealed record PositionPlanView(string Code, string NameText, string Channel, string DurationMinutes, string Cost, string Notes, string TeamText, string StatusLabel)
{
    public static PositionPlanView ToPositionPlanView(PositionPlan item)
    {
        return new PositionPlanView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Channel,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.Team, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionPlanView> ToPositionPlanViews(List<PositionPlan> items)
    {
        var views = new List<PositionPlanView>();
        foreach (var item in items)
        {
            views.Add(ToPositionPlanView(item));
        }
        return views;
    }

    public static double SumPositionPlanView(PositionPlanView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedCost;
    }
}
