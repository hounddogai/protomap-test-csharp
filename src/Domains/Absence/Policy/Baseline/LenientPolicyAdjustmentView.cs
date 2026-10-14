namespace Acme.Workforce.Absence.Policy.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient policy adjustment.</summary>
public sealed record LenientPolicyAdjustmentView(string CodeText, string Name, string Checksum, string Active, string Currency, string Locale, string Discount, string ManagerId, string Title, string Threshold, string StatusLabel)
{
    public static LenientPolicyAdjustmentView ToLenientPolicyAdjustmentView(LenientPolicyAdjustment item)
    {
        return new LenientPolicyAdjustmentView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Checksum,
            $"{item.Active}",
            Text.Truncate(item.Currency, 120),
            item.Locale,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPolicyAdjustmentView> ToLenientPolicyAdjustmentViews(List<LenientPolicyAdjustment> items)
    {
        var views = new List<LenientPolicyAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPolicyAdjustmentView(item));
        }
        return views;
    }

    public static double SumLenientPolicyAdjustmentView(LenientPolicyAdjustmentView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedThreshold;
    }
}
