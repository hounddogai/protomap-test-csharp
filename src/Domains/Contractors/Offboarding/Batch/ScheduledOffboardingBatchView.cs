namespace Acme.Workforce.Contractors.Offboarding.Batch;

using System.Globalization;

/// <summary>How screens and exports show a scheduled offboarding batch.</summary>
public sealed record ScheduledOffboardingBatchView(string CodeText, string Name, string Grade, string EmployeeIdText, string PriorityText, string BatchSizeText, string Visible, string RatioText, string LeaveDays, string Attempts, string StatusLabel)
{
    public static ScheduledOffboardingBatchView ToScheduledOffboardingBatchView(ScheduledOffboardingBatch item)
    {
        return new ScheduledOffboardingBatchView(
            item.Code,
            item.Name,
            item.Grade,
            item.EmployeeId,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledOffboardingBatchView> ToScheduledOffboardingBatchViews(List<ScheduledOffboardingBatch> items)
    {
        var views = new List<ScheduledOffboardingBatchView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledOffboardingBatchView(item));
        }
        return views;
    }

    public static double SumScheduledOffboardingBatchView(ScheduledOffboardingBatchView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedBatchSize + parsedRatio;
    }
}
