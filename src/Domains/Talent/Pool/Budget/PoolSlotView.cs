namespace Acme.Workforce.Talent.Pool.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool slot.</summary>
public sealed record PoolSlotView(string CodeText, string Label, string Locale, string Weight, string ManagerId, string FloorValue, string Badge, string SequenceText, string StatusLabel)
{
    public static PoolSlotView ToPoolSlotView(PoolSlot item)
    {
        return new PoolSlotView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 120),
            item.Locale,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolSlotView> ToPoolSlotViews(List<PoolSlot> items)
    {
        var views = new List<PoolSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPoolSlotView(item));
        }
        return views;
    }

    public static double SumPoolSlotView(PoolSlotView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedFloorValue + (double)parsedSequence;
    }
}
