namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement checkpoint.</summary>
public sealed record EngagementCheckpointView(string Code, string Title, string JobCodeText, string ManagerIdText, string Department, string SalaryBandText, string Label, string Priority, string Region, string StatusLabel)
{
    public static EngagementCheckpointView ToEngagementCheckpointView(EngagementCheckpoint item)
    {
        return new EngagementCheckpointView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.JobCode,
            Text.Truncate(item.ManagerId, 80),
            Text.Truncate(item.Department, 40),
            item.SalaryBand,
            Text.Truncate(item.Label, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementCheckpointView> ToEngagementCheckpointViews(List<EngagementCheckpoint> items)
    {
        var views = new List<EngagementCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementCheckpointView(item));
        }
        return views;
    }

    public static double SumEngagementCheckpointView(EngagementCheckpointView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedPriority;
    }
}
