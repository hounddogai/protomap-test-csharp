namespace Acme.Workforce.Compliance.Attestation.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation mapping.</summary>
public sealed record AttestationMappingView(string Code, string LabelText, string ExternalRefText, string Hours, string Score, string Weight, string Channel, string ActiveText, string StatusLabel)
{
    public static AttestationMappingView ToAttestationMappingView(AttestationMapping item)
    {
        return new AttestationMappingView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.ExternalRef,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationMappingView> ToAttestationMappingViews(List<AttestationMapping> items)
    {
        var views = new List<AttestationMappingView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationMappingView(item));
        }
        return views;
    }

    public static double SumAttestationMappingView(AttestationMappingView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedHours + parsedScore + parsedWeight;
    }
}
