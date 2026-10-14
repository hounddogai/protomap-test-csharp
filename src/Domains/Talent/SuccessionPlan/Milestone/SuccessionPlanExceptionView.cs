namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan exception.</summary>
public sealed record SuccessionPlanExceptionView(string CodeText, string Title, string CostText, string Weight, string GradeText, string TeamText, string CreatedAt, string SalaryBandText, string LeaveDays, string Unit, string JobCode, string StatusLabel)
{
    public static SuccessionPlanExceptionView ToSuccessionPlanExceptionView(SuccessionPlanException item)
    {
        return new SuccessionPlanExceptionView(
            item.Code,
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Team,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            Text.Truncate(item.JobCode, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanExceptionView> ToSuccessionPlanExceptionViews(List<SuccessionPlanException> items)
    {
        var views = new List<SuccessionPlanExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanExceptionView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanExceptionView(SuccessionPlanExceptionView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedCost + parsedWeight + (double)parsedCreatedAt;
    }
}
