namespace Acme.Workforce.Talent.Pool.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a adaptive pool assignment.</summary>
public sealed record AdaptivePoolAssignmentView(string Code, string Label, string CapacityText, string MinimumText, string Region, string GraceDaysText, string StatusLabel)
{
    public static AdaptivePoolAssignmentView ToAdaptivePoolAssignmentView(AdaptivePoolAssignment item)
    {
        return new AdaptivePoolAssignmentView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePoolAssignmentView> ToAdaptivePoolAssignmentViews(List<AdaptivePoolAssignment> items)
    {
        var views = new List<AdaptivePoolAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePoolAssignmentView(item));
        }
        return views;
    }

    public static double SumAdaptivePoolAssignmentView(AdaptivePoolAssignmentView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedMinimum + (double)parsedGraceDays;
    }
}
