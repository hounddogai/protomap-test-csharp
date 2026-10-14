namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending attestation threshold.</summary>
public sealed record PendingAttestationThresholdView(string Code, string Label, string Department, string ManagerId, string Quantity, string StatusLabel)
{
    public static PendingAttestationThresholdView ToPendingAttestationThresholdView(PendingAttestationThreshold item)
    {
        return new PendingAttestationThresholdView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Department,
            item.ManagerId,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAttestationThresholdView> ToPendingAttestationThresholdViews(List<PendingAttestationThreshold> items)
    {
        var views = new List<PendingAttestationThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAttestationThresholdView(item));
        }
        return views;
    }

    public static double SumPendingAttestationThresholdView(PendingAttestationThresholdView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
