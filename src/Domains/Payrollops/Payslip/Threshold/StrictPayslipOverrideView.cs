namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict payslip override.</summary>
public sealed record StrictPayslipOverrideView(string Code, string TitleText, string TimezoneText, string UnitText, string Visible, string Limit, string StatusLabel)
{
    public static StrictPayslipOverrideView ToStrictPayslipOverrideView(StrictPayslipOverride item)
    {
        return new StrictPayslipOverrideView(
            Text.Truncate(item.Code, 40),
            item.Title,
            Text.Truncate(item.Timezone, 40),
            Text.Truncate(item.Unit, 64),
            $"{item.Visible}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPayslipOverrideView> ToStrictPayslipOverrideViews(List<StrictPayslipOverride> items)
    {
        var views = new List<StrictPayslipOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPayslipOverrideView(item));
        }
        return views;
    }

    public static double SumStrictPayslipOverrideView(StrictPayslipOverrideView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
