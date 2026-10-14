namespace Acme.Workforce.Compliance.Exception.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending exception plan.</summary>
public sealed record PendingExceptionPlanView(string CodeText, string Title, string BatchSize, string ReviewerText, string ApprovedText, string Segment, string StatusLabel)
{
    public static PendingExceptionPlanView ToPendingExceptionPlanView(PendingExceptionPlan item)
    {
        return new PendingExceptionPlanView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Approved}",
            Text.Truncate(item.Segment, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingExceptionPlanView> ToPendingExceptionPlanViews(List<PendingExceptionPlan> items)
    {
        var views = new List<PendingExceptionPlanView>();
        foreach (var item in items)
        {
            views.Add(ToPendingExceptionPlanView(item));
        }
        return views;
    }

    public static double SumPendingExceptionPlanView(PendingExceptionPlanView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}
