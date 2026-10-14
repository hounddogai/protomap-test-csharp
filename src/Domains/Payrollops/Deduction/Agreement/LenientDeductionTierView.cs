namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient deduction tier.</summary>
public sealed record LenientDeductionTierView(string CodeText, string Name, string EmployeeId, string PhoneText, string Threshold, string Approved, string Margin, string ManagerIdText, string Location, string StatusLabel)
{
    public static LenientDeductionTierView ToLenientDeductionTierView(LenientDeductionTier item)
    {
        return new LenientDeductionTierView(
            item.Code,
            item.Name,
            item.EmployeeId,
            item.Phone,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 64),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientDeductionTierView> ToLenientDeductionTierViews(List<LenientDeductionTier> items)
    {
        var views = new List<LenientDeductionTierView>();
        foreach (var item in items)
        {
            views.Add(ToLenientDeductionTierView(item));
        }
        return views;
    }

    public static double SumLenientDeductionTierView(LenientDeductionTierView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedMargin;
    }
}
