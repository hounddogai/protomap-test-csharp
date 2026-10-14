namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional holiday policy.</summary>
public sealed record RegionalHolidayPolicyView(string Code, string NameText, string RetriesText, string Channel, string ReviewerText, string CurrencyText, string WindowDaysText, string Sequence, string LocaleText, string Pinned, string BadgeText, string StatusLabel)
{
    public static RegionalHolidayPolicyView ToRegionalHolidayPolicyView(RegionalHolidayPolicy item)
    {
        return new RegionalHolidayPolicyView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            Text.Truncate(item.Reviewer, 40),
            item.Currency,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Pinned}",
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalHolidayPolicyView> ToRegionalHolidayPolicyViews(List<RegionalHolidayPolicy> items)
    {
        var views = new List<RegionalHolidayPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalHolidayPolicyView(item));
        }
        return views;
    }

    public static double SumRegionalHolidayPolicyView(RegionalHolidayPolicyView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedWindowDays + (double)parsedSequence;
    }
}
