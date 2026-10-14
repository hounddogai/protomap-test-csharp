namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal payslip baseline.</summary>
public sealed record SeasonalPayslipBaselineView(string Code, string Label, string CapacityText, string Department, string Currency, string Skill, string Country, string WeightText, string GradeText, string ContactEmail, string StatusLabel)
{
    public static SeasonalPayslipBaselineView ToSeasonalPayslipBaselineView(SeasonalPayslipBaseline item)
    {
        return new SeasonalPayslipBaselineView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Currency,
            Text.Truncate(item.Skill, 120),
            item.Country,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPayslipBaselineView> ToSeasonalPayslipBaselineViews(List<SeasonalPayslipBaseline> items)
    {
        var views = new List<SeasonalPayslipBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPayslipBaselineView(item));
        }
        return views;
    }

    public static double SumSeasonalPayslipBaselineView(SeasonalPayslipBaselineView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedWeight;
    }
}
