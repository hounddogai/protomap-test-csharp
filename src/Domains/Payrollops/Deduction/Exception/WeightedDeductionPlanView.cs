namespace Acme.Workforce.Payrollops.Deduction.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted deduction plan.</summary>
public sealed record WeightedDeductionPlanView(string Code, string Label, string Locale, string VersionText, string Checksum, string Name, string Threshold, string Unit, string Timezone, string EmployeeId, string StatusLabel)
{
    public static WeightedDeductionPlanView ToWeightedDeductionPlanView(WeightedDeductionPlan item)
    {
        return new WeightedDeductionPlanView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Locale, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            Text.Truncate(item.Name, 120),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.Timezone,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedDeductionPlanView> ToWeightedDeductionPlanViews(List<WeightedDeductionPlan> items)
    {
        var views = new List<WeightedDeductionPlanView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedDeductionPlanView(item));
        }
        return views;
    }

    public static double SumWeightedDeductionPlanView(WeightedDeductionPlanView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedThreshold;
    }
}
