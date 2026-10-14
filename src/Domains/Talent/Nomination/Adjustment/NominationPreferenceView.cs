namespace Acme.Workforce.Talent.Nomination.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a nomination preference.</summary>
public sealed record NominationPreferenceView(string CodeText, string NameText, string CategoryText, string VersionText, string LeaveDays, string StatusLabel)
{
    public static NominationPreferenceView ToNominationPreferenceView(NominationPreference item)
    {
        return new NominationPreferenceView(
            item.Code,
            item.Name,
            item.Category,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationPreferenceView> ToNominationPreferenceViews(List<NominationPreference> items)
    {
        var views = new List<NominationPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToNominationPreferenceView(item));
        }
        return views;
    }

    public static double SumNominationPreferenceView(NominationPreferenceView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedLeaveDays;
    }
}
