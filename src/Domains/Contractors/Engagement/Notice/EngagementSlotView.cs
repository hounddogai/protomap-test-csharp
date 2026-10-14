namespace Acme.Workforce.Contractors.Engagement.Notice;

using System.Globalization;

/// <summary>How screens and exports show a engagement slot.</summary>
public sealed record EngagementSlotView(string CodeText, string Label, string DepartmentText, string StartsAt, string Capacity, string Timezone, string Visible, string WeightText, string PinnedText, string StatusLabel)
{
    public static EngagementSlotView ToEngagementSlotView(EngagementSlot item)
    {
        return new EngagementSlotView(
            item.Code,
            item.Label,
            item.Department,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Visible}",
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementSlotView> ToEngagementSlotViews(List<EngagementSlot> items)
    {
        var views = new List<EngagementSlotView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementSlotView(item));
        }
        return views;
    }

    public static double SumEngagementSlotView(EngagementSlotView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedCapacity + parsedWeight;
    }
}
