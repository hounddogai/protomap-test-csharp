namespace Acme.Workforce.Compensation.BonusPool.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback bonus pool schedule.</summary>
public sealed record FallbackBonusPoolScheduleView(string Code, string Name, string Version, string EmployeeIdText, string BadgeText, string ApprovedText, string EndsAt, string StatusLabel)
{
    public static FallbackBonusPoolScheduleView ToFallbackBonusPoolScheduleView(FallbackBonusPoolSchedule item)
    {
        return new FallbackBonusPoolScheduleView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 80),
            item.Badge,
            $"{item.Approved}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackBonusPoolScheduleView> ToFallbackBonusPoolScheduleViews(List<FallbackBonusPoolSchedule> items)
    {
        var views = new List<FallbackBonusPoolScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackBonusPoolScheduleView(item));
        }
        return views;
    }

    public static double SumFallbackBonusPoolScheduleView(FallbackBonusPoolScheduleView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedEndsAt;
    }
}
