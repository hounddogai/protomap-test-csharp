namespace Acme.Workforce.Compliance.Attestation.Rule;

using System.Globalization;

/// <summary>How screens and exports show a attestation rule.</summary>
public sealed record AttestationRuleView(string CodeText, string Title, string RateText, string Archived, string Rating, string StatusLabel)
{
    public static AttestationRuleView ToAttestationRuleView(AttestationRule item)
    {
        return new AttestationRuleView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationRuleView> ToAttestationRuleViews(List<AttestationRule> items)
    {
        var views = new List<AttestationRuleView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationRuleView(item));
        }
        return views;
    }

    public static double SumAttestationRuleView(AttestationRuleView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRate + parsedRating;
    }
}
