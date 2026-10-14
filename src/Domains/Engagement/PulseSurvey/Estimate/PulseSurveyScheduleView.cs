namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey schedule.</summary>
public sealed record PulseSurveyScheduleView(string Code, string TitleText, string Discount, string Currency, string EmployeeId, string Name, string StartsAt, string Capacity, string Checksum, string Sequence, string Archived, string StatusLabel)
{
    public static PulseSurveyScheduleView ToPulseSurveyScheduleView(PulseSurveySchedule item)
    {
        return new PulseSurveyScheduleView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            item.EmployeeId,
            Text.Truncate(item.Name, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyScheduleView> ToPulseSurveyScheduleViews(List<PulseSurveySchedule> items)
    {
        var views = new List<PulseSurveyScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyScheduleView(item));
        }
        return views;
    }

    public static double SumPulseSurveyScheduleView(PulseSurveyScheduleView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedStartsAt + (double)parsedCapacity;
    }
}
