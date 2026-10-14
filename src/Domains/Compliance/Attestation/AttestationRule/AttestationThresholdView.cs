namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation threshold.</summary>
public sealed record AttestationThresholdView(string CodeText, string Name, string ExternalRef, string Budget, string CreatedAt, string UpdatedAtText, string Channel, string StatusLabel)
{
    public static AttestationThresholdView ToAttestationThresholdView(AttestationThreshold item)
    {
        return new AttestationThresholdView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.ExternalRef,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationThresholdView> ToAttestationThresholdViews(List<AttestationThreshold> items)
    {
        var views = new List<AttestationThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationThresholdView(item));
        }
        return views;
    }

    public static double SumAttestationThresholdView(AttestationThresholdView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedCreatedAt + (double)parsedUpdatedAt;
    }
}
