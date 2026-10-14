namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using System.Globalization;

/// <summary>How screens and exports show a succession plan schedule.</summary>
public sealed record SuccessionPlanScheduleView(string Code, string Label, string Approved, string Sequence, string Margin, string StatusLabel)
{
    public static SuccessionPlanScheduleView ToSuccessionPlanScheduleView(SuccessionPlanSchedule item)
    {
        return new SuccessionPlanScheduleView(
            item.Code,
            item.Label,
            $"{item.Approved}",
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanScheduleView> ToSuccessionPlanScheduleViews(List<SuccessionPlanSchedule> items)
    {
        var views = new List<SuccessionPlanScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanScheduleView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanScheduleView(SuccessionPlanScheduleView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedMargin;
    }
}
