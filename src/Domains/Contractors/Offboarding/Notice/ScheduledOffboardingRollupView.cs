namespace Acme.Workforce.Contractors.Offboarding.Notice;

using System.Globalization;

/// <summary>How screens and exports show a scheduled offboarding rollup.</summary>
public sealed record ScheduledOffboardingRollupView(string Code, string TitleText, string TeamText, string Locale, string Country, string Region, string AmountText, string Revision, string EndsAt, string StatusLabel)
{
    public static ScheduledOffboardingRollupView ToScheduledOffboardingRollupView(ScheduledOffboardingRollup item)
    {
        return new ScheduledOffboardingRollupView(
            item.Code,
            item.Title,
            item.Team,
            item.Locale,
            item.Country,
            item.Region,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledOffboardingRollupView> ToScheduledOffboardingRollupViews(List<ScheduledOffboardingRollup> items)
    {
        var views = new List<ScheduledOffboardingRollupView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledOffboardingRollupView(item));
        }
        return views;
    }

    public static double SumScheduledOffboardingRollupView(ScheduledOffboardingRollupView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedRevision + (double)parsedEndsAt;
    }
}
