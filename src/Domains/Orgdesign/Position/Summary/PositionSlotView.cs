namespace Acme.Workforce.Orgdesign.Position.Summary;

using System.Globalization;

/// <summary>How screens and exports show a position slot.</summary>
public sealed record PositionSlotView(string Code, string Name, string ChannelText, string Capacity, string ChecksumText, string ContactEmail, string Badge, string Attempts, string UpdatedAt, string Sequence, string StatusLabel)
{
    public static PositionSlotView ToPositionSlotView(PositionSlot item)
    {
        return new PositionSlotView(
            item.Code,
            item.Name,
            item.Channel,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.ContactEmail,
            item.Badge,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionSlotView> ToPositionSlotViews(List<PositionSlot> items)
    {
        var views = new List<PositionSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPositionSlotView(item));
        }
        return views;
    }

    public static double SumPositionSlotView(PositionSlotView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedAttempts + (double)parsedUpdatedAt;
    }
}
