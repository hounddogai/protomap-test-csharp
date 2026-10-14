namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a merit cycle milestone.</summary>
public sealed record MeritCycleMilestoneView(string Code, string LabelText, string Summary, string Visible, string Threshold, string BatchSizeText, string RevisionText, string ExternalRefText, string StatusLabel)
{
    public static MeritCycleMilestoneView ToMeritCycleMilestoneView(MeritCycleMilestone item)
    {
        return new MeritCycleMilestoneView(
            item.Code,
            item.Label,
            item.Summary,
            $"{item.Visible}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleMilestoneView> ToMeritCycleMilestoneViews(List<MeritCycleMilestone> items)
    {
        var views = new List<MeritCycleMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleMilestoneView(item));
        }
        return views;
    }

    public static double SumMeritCycleMilestoneView(MeritCycleMilestoneView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedBatchSize + (double)parsedRevision;
    }
}
