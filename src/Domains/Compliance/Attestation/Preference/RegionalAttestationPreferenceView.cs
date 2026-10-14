namespace Acme.Workforce.Compliance.Attestation.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional attestation preference.</summary>
public sealed record RegionalAttestationPreferenceView(string Code, string TitleText, string TimezoneText, string ExpiresAt, string Budget, string SequenceText, string ScoreText, string StatusLabel)
{
    public static RegionalAttestationPreferenceView ToRegionalAttestationPreferenceView(RegionalAttestationPreference item)
    {
        return new RegionalAttestationPreferenceView(
            item.Code,
            item.Title,
            Text.Truncate(item.Timezone, 40),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAttestationPreferenceView> ToRegionalAttestationPreferenceViews(List<RegionalAttestationPreference> items)
    {
        var views = new List<RegionalAttestationPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAttestationPreferenceView(item));
        }
        return views;
    }

    public static double SumRegionalAttestationPreferenceView(RegionalAttestationPreferenceView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedBudget + (double)parsedSequence;
    }
}
