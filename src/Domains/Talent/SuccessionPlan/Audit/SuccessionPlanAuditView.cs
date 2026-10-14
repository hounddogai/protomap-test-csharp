namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan audit.</summary>
public sealed record SuccessionPlanAuditView(string Code, string Name, string Active, string GradeText, string Capacity, string RetriesText, string Sequence, string SegmentText, string StatusLabel)
{
    public static SuccessionPlanAuditView ToSuccessionPlanAuditView(SuccessionPlanAudit item)
    {
        return new SuccessionPlanAuditView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Active}",
            item.Grade,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanAuditView> ToSuccessionPlanAuditViews(List<SuccessionPlanAudit> items)
    {
        var views = new List<SuccessionPlanAuditView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanAuditView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanAuditView(SuccessionPlanAuditView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedRetries + (double)parsedSequence;
    }
}
