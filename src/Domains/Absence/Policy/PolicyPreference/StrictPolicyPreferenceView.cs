namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict policy preference.</summary>
public sealed record StrictPolicyPreferenceView(string Code, string Title, string Attempts, string Channel, string Source, string Sequence, string TenureMonthsText, string Ratio, string EmployeeId, string Amount, string Fte, string StatusLabel)
{
    public static StrictPolicyPreferenceView ToStrictPolicyPreferenceView(StrictPolicyPreference item)
    {
        return new StrictPolicyPreferenceView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Source,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPolicyPreferenceView> ToStrictPolicyPreferenceViews(List<StrictPolicyPreference> items)
    {
        var views = new List<StrictPolicyPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPolicyPreferenceView(item));
        }
        return views;
    }

    public static double SumStrictPolicyPreferenceView(StrictPolicyPreferenceView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedSequence + (double)parsedTenureMonths;
    }
}
