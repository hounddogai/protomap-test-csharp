namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle template.</summary>
public sealed record MeritCycleTemplateView(string CodeText, string TitleText, string DurationMinutes, string CostCenter, string RatingText, string Location, string JobCode, string FloorValue, string Team, string LeaveDays, string Badge, string StatusLabel)
{
    public static MeritCycleTemplateView ToMeritCycleTemplateView(MeritCycleTemplate item)
    {
        return new MeritCycleTemplateView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.JobCode,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleTemplateView> ToMeritCycleTemplateViews(List<MeritCycleTemplate> items)
    {
        var views = new List<MeritCycleTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleTemplateView(item));
        }
        return views;
    }

    public static double SumMeritCycleTemplateView(MeritCycleTemplateView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRating + parsedFloorValue;
    }
}
