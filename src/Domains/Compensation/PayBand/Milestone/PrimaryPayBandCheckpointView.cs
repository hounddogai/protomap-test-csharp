namespace Acme.Workforce.Compensation.PayBand.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary pay band checkpoint.</summary>
public sealed record PrimaryPayBandCheckpointView(string Code, string Label, string LeaveDays, string Phone, string Badge, string StatusLabel)
{
    public static PrimaryPayBandCheckpointView ToPrimaryPayBandCheckpointView(PrimaryPayBandCheckpoint item)
    {
        return new PrimaryPayBandCheckpointView(
            item.Code,
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            Text.Truncate(item.Badge, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPayBandCheckpointView> ToPrimaryPayBandCheckpointViews(List<PrimaryPayBandCheckpoint> items)
    {
        var views = new List<PrimaryPayBandCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPayBandCheckpointView(item));
        }
        return views;
    }

    public static double SumPrimaryPayBandCheckpointView(PrimaryPayBandCheckpointView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
