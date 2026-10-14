namespace Acme.Workforce.Compliance.Attestation.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation override.</summary>
public sealed record AttestationOverrideView(string Code, string LabelText, string GraceDays, string BatchSizeText, string Threshold, string Visible, string StatusLabel)
{
    public static AttestationOverrideView ToAttestationOverrideView(AttestationOverride item)
    {
        return new AttestationOverrideView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationOverrideView> ToAttestationOverrideViews(List<AttestationOverride> items)
    {
        var views = new List<AttestationOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationOverrideView(item));
        }
        return views;
    }

    public static double SumAttestationOverrideView(AttestationOverrideView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedBatchSize + parsedThreshold;
    }
}
