namespace Acme.Workforce.Absence.Policy.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy baseline.</summary>
public sealed record PolicyBaselineView(string Code, string NameText, string Capacity, string MaximumText, string LabelText, string FloorValue, string DurationMinutes, string AmountText, string Ratio, string Fte, string StatusLabel)
{
    public static PolicyBaselineView ToPolicyBaselineView(PolicyBaseline item)
    {
        return new PolicyBaselineView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyBaselineView> ToPolicyBaselineViews(List<PolicyBaseline> items)
    {
        var views = new List<PolicyBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyBaselineView(item));
        }
        return views;
    }

    public static double SumPolicyBaselineView(PolicyBaselineView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedMaximum + parsedFloorValue;
    }
}
