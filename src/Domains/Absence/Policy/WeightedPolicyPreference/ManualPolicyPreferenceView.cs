namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual policy preference.</summary>
public sealed record ManualPolicyPreferenceView(string Code, string LabelText, string SummaryText, string MarginText, string MinimumText, string ExpiresAt, string Revision, string StartsAtText, string WindowDaysText, string Retries, string LocationText, string StatusLabel)
{
    public static ManualPolicyPreferenceView ToManualPolicyPreferenceView(ManualPolicyPreference item)
    {
        return new ManualPolicyPreferenceView(
            item.Code,
            item.Label,
            Text.Truncate(item.Summary, 40),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPolicyPreferenceView> ToManualPolicyPreferenceViews(List<ManualPolicyPreference> items)
    {
        var views = new List<ManualPolicyPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToManualPolicyPreferenceView(item));
        }
        return views;
    }

    public static double SumManualPolicyPreferenceView(ManualPolicyPreferenceView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedMargin + parsedMinimum + (double)parsedExpiresAt;
    }
}
