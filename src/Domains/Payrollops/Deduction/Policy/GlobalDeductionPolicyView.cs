namespace Acme.Workforce.Payrollops.Deduction.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global deduction policy.</summary>
public sealed record GlobalDeductionPolicyView(string Code, string Name, string Country, string Channel, string CurrencyText, string CapacityText, string QuantityText, string Sequence, string Grade, string Region, string CreatedAt, string StatusLabel)
{
    public static GlobalDeductionPolicyView ToGlobalDeductionPolicyView(GlobalDeductionPolicy item)
    {
        return new GlobalDeductionPolicyView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Country,
            item.Channel,
            Text.Truncate(item.Currency, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            Text.Truncate(item.Region, 120),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalDeductionPolicyView> ToGlobalDeductionPolicyViews(List<GlobalDeductionPolicy> items)
    {
        var views = new List<GlobalDeductionPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalDeductionPolicyView(item));
        }
        return views;
    }

    public static double SumGlobalDeductionPolicyView(GlobalDeductionPolicyView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedQuantity + (double)parsedSequence;
    }
}
