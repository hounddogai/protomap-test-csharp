namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient succession plan record.</summary>
public sealed record LenientSuccessionPlanRecordView(string Code, string Name, string Cost, string DepartmentText, string PinnedText, string Threshold, string Sequence, string RatingText, string CeilingText, string RetriesText, string Visible, string StatusLabel)
{
    public static LenientSuccessionPlanRecordView ToLenientSuccessionPlanRecordView(LenientSuccessionPlanRecord item)
    {
        return new LenientSuccessionPlanRecordView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Pinned}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientSuccessionPlanRecordView> ToLenientSuccessionPlanRecordViews(List<LenientSuccessionPlanRecord> items)
    {
        var views = new List<LenientSuccessionPlanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToLenientSuccessionPlanRecordView(item));
        }
        return views;
    }

    public static double SumLenientSuccessionPlanRecordView(LenientSuccessionPlanRecordView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedCost + parsedThreshold + (double)parsedSequence;
    }
}
