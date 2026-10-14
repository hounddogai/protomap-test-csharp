namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using System.Globalization;

/// <summary>How screens and exports show a manual merit cycle override.</summary>
public sealed record ManualMeritCycleOverrideView(string Code, string LabelText, string Reviewer, string GraceDays, string Margin, string SequenceText, string ManagerIdText, string Threshold, string OwnerId, string Weight, string StatusLabel)
{
    public static ManualMeritCycleOverrideView ToManualMeritCycleOverrideView(ManualMeritCycleOverride item)
    {
        return new ManualMeritCycleOverrideView(
            item.Code,
            item.Label,
            item.Reviewer,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualMeritCycleOverrideView> ToManualMeritCycleOverrideViews(List<ManualMeritCycleOverride> items)
    {
        var views = new List<ManualMeritCycleOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToManualMeritCycleOverrideView(item));
        }
        return views;
    }

    public static double SumManualMeritCycleOverrideView(ManualMeritCycleOverrideView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedMargin + (double)parsedSequence;
    }
}
