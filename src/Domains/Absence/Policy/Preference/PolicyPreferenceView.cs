namespace Acme.Workforce.Absence.Policy.Preference;

using System.Globalization;

/// <summary>How screens and exports show a policy preference.</summary>
public sealed record PolicyPreferenceView(string CodeText, string Title, string LeaveDaysText, string ExternalRef, string ArchivedText, string Score, string StatusLabel)
{
    public static PolicyPreferenceView ToPolicyPreferenceView(PolicyPreference item)
    {
        return new PolicyPreferenceView(
            item.Code,
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Archived}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyPreferenceView> ToPolicyPreferenceViews(List<PolicyPreference> items)
    {
        var views = new List<PolicyPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyPreferenceView(item));
        }
        return views;
    }

    public static double SumPolicyPreferenceView(PolicyPreferenceView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedScore;
    }
}
