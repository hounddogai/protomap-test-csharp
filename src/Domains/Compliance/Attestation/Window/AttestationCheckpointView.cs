namespace Acme.Workforce.Compliance.Attestation.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation checkpoint.</summary>
public sealed record AttestationCheckpointView(string Code, string Label, string FloorValue, string VisibleText, string Budget, string Currency, string Minimum, string ManagerId, string StatusLabel)
{
    public static AttestationCheckpointView ToAttestationCheckpointView(AttestationCheckpoint item)
    {
        return new AttestationCheckpointView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationCheckpointView> ToAttestationCheckpointViews(List<AttestationCheckpoint> items)
    {
        var views = new List<AttestationCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationCheckpointView(item));
        }
        return views;
    }

    public static double SumAttestationCheckpointView(AttestationCheckpointView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedBudget + parsedMinimum;
    }
}
