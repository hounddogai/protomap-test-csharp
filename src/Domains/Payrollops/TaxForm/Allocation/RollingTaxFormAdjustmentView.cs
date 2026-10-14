namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling tax form adjustment.</summary>
public sealed record RollingTaxFormAdjustmentView(string Code, string Name, string Region, string PinnedText, string Attempts, string Grade, string Capacity, string StatusLabel)
{
    public static RollingTaxFormAdjustmentView ToRollingTaxFormAdjustmentView(RollingTaxFormAdjustment item)
    {
        return new RollingTaxFormAdjustmentView(
            item.Code,
            item.Name,
            Text.Truncate(item.Region, 40),
            $"{item.Pinned}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingTaxFormAdjustmentView> ToRollingTaxFormAdjustmentViews(List<RollingTaxFormAdjustment> items)
    {
        var views = new List<RollingTaxFormAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRollingTaxFormAdjustmentView(item));
        }
        return views;
    }

    public static double SumRollingTaxFormAdjustmentView(RollingTaxFormAdjustmentView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedCapacity;
    }
}
