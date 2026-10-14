namespace Acme.Workforce.Compensation.EquityGrant.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant group.</summary>
public sealed record EquityGrantGroupView(string Code, string LabelText, string Weight, string Channel, string Sequence, string ExternalRef, string FloorValue, string UpdatedAt, string DurationMinutes, string TenureMonths, string Revision, string StatusLabel)
{
    public static EquityGrantGroupView ToEquityGrantGroupView(EquityGrantGroup item)
    {
        return new EquityGrantGroupView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantGroupView> ToEquityGrantGroupViews(List<EquityGrantGroup> items)
    {
        var views = new List<EquityGrantGroupView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantGroupView(item));
        }
        return views;
    }

    public static double SumEquityGrantGroupView(EquityGrantGroupView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedSequence + parsedFloorValue;
    }
}
