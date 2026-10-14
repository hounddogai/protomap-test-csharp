namespace Acme.Workforce.Orgdesign.Span.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict span slot.</summary>
public sealed record StrictSpanSlotView(string Code, string Label, string LeaveDays, string Unit, string Amount, string StatusLabel)
{
    public static StrictSpanSlotView ToStrictSpanSlotView(StrictSpanSlot item)
    {
        return new StrictSpanSlotView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictSpanSlotView> ToStrictSpanSlotViews(List<StrictSpanSlot> items)
    {
        var views = new List<StrictSpanSlotView>();
        foreach (var item in items)
        {
            views.Add(ToStrictSpanSlotView(item));
        }
        return views;
    }

    public static double SumStrictSpanSlotView(StrictSpanSlotView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedAmount;
    }
}
