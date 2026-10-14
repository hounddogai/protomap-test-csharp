namespace Acme.Workforce.Payrollops.Deduction.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped deduction policy.</summary>
public sealed record CappedDeductionPolicyView(string Code, string Name, string Phone, string WindowDays, string DescriptionText, string Version, string CategoryText, string Currency, string Score, string EmployeeIdText, string StatusLabel)
{
    public static CappedDeductionPolicyView ToCappedDeductionPolicyView(CappedDeductionPolicy item)
    {
        return new CappedDeductionPolicyView(
            item.Code,
            item.Name,
            item.Phone,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 120),
            Text.Truncate(item.Currency, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedDeductionPolicyView> ToCappedDeductionPolicyViews(List<CappedDeductionPolicy> items)
    {
        var views = new List<CappedDeductionPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToCappedDeductionPolicyView(item));
        }
        return views;
    }

    public static double SumCappedDeductionPolicyView(CappedDeductionPolicyView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedVersion + parsedScore;
    }
}
