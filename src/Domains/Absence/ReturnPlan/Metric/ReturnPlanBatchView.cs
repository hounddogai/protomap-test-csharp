namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan batch.</summary>
public sealed record ReturnPlanBatchView(string Code, string Label, string Title, string OwnerId, string Version, string StatusLabel)
{
    public static ReturnPlanBatchView ToReturnPlanBatchView(ReturnPlanBatch item)
    {
        return new ReturnPlanBatchView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Title,
            item.OwnerId,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanBatchView> ToReturnPlanBatchViews(List<ReturnPlanBatch> items)
    {
        var views = new List<ReturnPlanBatchView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanBatchView(item));
        }
        return views;
    }

    public static double SumReturnPlanBatchView(ReturnPlanBatchView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedVersion;
    }
}
