namespace Acme.Workforce.Absence.ReturnPlan.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict return plan slot.</summary>
public sealed record StrictReturnPlanSlotView(string Code, string TitleText, string SequenceText, string Limit, string Quantity, string ContactEmail, string Country, string CostCenterText, string Hours, string StatusLabel)
{
    public static StrictReturnPlanSlotView ToStrictReturnPlanSlotView(StrictReturnPlanSlot item)
    {
        return new StrictReturnPlanSlotView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 64),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            item.Country,
            Text.Truncate(item.CostCenter, 120),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictReturnPlanSlotView> ToStrictReturnPlanSlotViews(List<StrictReturnPlanSlot> items)
    {
        var views = new List<StrictReturnPlanSlotView>();
        foreach (var item in items)
        {
            views.Add(ToStrictReturnPlanSlotView(item));
        }
        return views;
    }

    public static double SumStrictReturnPlanSlotView(StrictReturnPlanSlotView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedLimit + (double)parsedQuantity;
    }
}
