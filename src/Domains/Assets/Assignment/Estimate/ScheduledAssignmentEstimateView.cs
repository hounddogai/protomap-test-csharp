namespace Acme.Workforce.Assets.Assignment.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled assignment estimate.</summary>
public sealed record ScheduledAssignmentEstimateView(string Code, string Label, string SalaryBand, string Retries, string Revision, string TenureMonths, string Skill, string StatusLabel)
{
    public static ScheduledAssignmentEstimateView ToScheduledAssignmentEstimateView(ScheduledAssignmentEstimate item)
    {
        return new ScheduledAssignmentEstimateView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.SalaryBand,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAssignmentEstimateView> ToScheduledAssignmentEstimateViews(List<ScheduledAssignmentEstimate> items)
    {
        var views = new List<ScheduledAssignmentEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAssignmentEstimateView(item));
        }
        return views;
    }

    public static double SumScheduledAssignmentEstimateView(ScheduledAssignmentEstimateView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedRevision + (double)parsedTenureMonths;
    }
}
