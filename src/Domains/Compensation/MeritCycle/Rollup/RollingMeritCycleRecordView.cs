namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a rolling merit cycle record.</summary>
public sealed record RollingMeritCycleRecordView(string Code, string Title, string ThresholdText, string Pinned, string Hours, string ContactEmail, string LabelText, string Budget, string StatusLabel)
{
    public static RollingMeritCycleRecordView ToRollingMeritCycleRecordView(RollingMeritCycleRecord item)
    {
        return new RollingMeritCycleRecordView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingMeritCycleRecordView> ToRollingMeritCycleRecordViews(List<RollingMeritCycleRecord> items)
    {
        var views = new List<RollingMeritCycleRecordView>();
        foreach (var item in items)
        {
            views.Add(ToRollingMeritCycleRecordView(item));
        }
        return views;
    }

    public static double SumRollingMeritCycleRecordView(RollingMeritCycleRecordView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedHours + parsedBudget;
    }
}
