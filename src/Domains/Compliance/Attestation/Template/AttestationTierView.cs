namespace Acme.Workforce.Compliance.Attestation.Template;

using System.Globalization;

/// <summary>How screens and exports show a attestation tier.</summary>
public sealed record AttestationTierView(string Code, string Label, string Score, string Archived, string WindowDaysText, string Limit, string Sequence, string ReviewerText, string Amount, string StatusLabel)
{
    public static AttestationTierView ToAttestationTierView(AttestationTier item)
    {
        return new AttestationTierView(
            item.Code,
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationTierView> ToAttestationTierViews(List<AttestationTier> items)
    {
        var views = new List<AttestationTierView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationTierView(item));
        }
        return views;
    }

    public static double SumAttestationTierView(AttestationTierView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedWindowDays + (double)parsedLimit;
    }
}
