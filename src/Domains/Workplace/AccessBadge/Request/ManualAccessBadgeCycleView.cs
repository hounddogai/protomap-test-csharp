namespace Acme.Workforce.Workplace.AccessBadge.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual access badge cycle.</summary>
public sealed record ManualAccessBadgeCycleView(string Code, string Label, string SegmentText, string JobCodeText, string ThresholdText, string StatusLabel)
{
    public static ManualAccessBadgeCycleView ToManualAccessBadgeCycleView(ManualAccessBadgeCycle item)
    {
        return new ManualAccessBadgeCycleView(
            item.Code,
            item.Label,
            Text.Truncate(item.Segment, 120),
            item.JobCode,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAccessBadgeCycleView> ToManualAccessBadgeCycleViews(List<ManualAccessBadgeCycle> items)
    {
        var views = new List<ManualAccessBadgeCycleView>();
        foreach (var item in items)
        {
            views.Add(ToManualAccessBadgeCycleView(item));
        }
        return views;
    }

    public static double SumManualAccessBadgeCycleView(ManualAccessBadgeCycleView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
