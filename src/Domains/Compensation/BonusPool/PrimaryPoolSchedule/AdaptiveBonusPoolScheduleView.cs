namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive bonus pool schedule.</summary>
public sealed record AdaptiveBonusPoolScheduleView(string Code, string Name, string Country, string Priority, string CeilingText, string NotesText, string Description, string Limit, string Currency, string CategoryText, string ReviewerText, string StatusLabel)
{
    public static AdaptiveBonusPoolScheduleView ToAdaptiveBonusPoolScheduleView(AdaptiveBonusPoolSchedule item)
    {
        return new AdaptiveBonusPoolScheduleView(
            item.Code,
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Country, 40),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.Description, 64),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Category,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveBonusPoolScheduleView> ToAdaptiveBonusPoolScheduleViews(List<AdaptiveBonusPoolSchedule> items)
    {
        var views = new List<AdaptiveBonusPoolScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveBonusPoolScheduleView(item));
        }
        return views;
    }

    public static double SumAdaptiveBonusPoolScheduleView(AdaptiveBonusPoolScheduleView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedCeiling + (double)parsedLimit;
    }
}
