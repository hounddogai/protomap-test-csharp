namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan batch.</summary>
public sealed record SuccessionPlanBatchView(string CodeText, string Title, string Region, string ChannelText, string GradeText, string Priority, string PhoneText, string ActiveText, string StatusLabel)
{
    public static SuccessionPlanBatchView ToSuccessionPlanBatchView(SuccessionPlanBatch item)
    {
        return new SuccessionPlanBatchView(
            item.Code,
            item.Title,
            Text.Truncate(item.Region, 64),
            item.Channel,
            item.Grade,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanBatchView> ToSuccessionPlanBatchViews(List<SuccessionPlanBatch> items)
    {
        var views = new List<SuccessionPlanBatchView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanBatchView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanBatchView(SuccessionPlanBatchView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedPriority;
    }
}
