namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment checkpoint.</summary>
public sealed record AssignmentCheckpointView(string CodeText, string Label, string CreatedAt, string SkillText, string Version, string StartsAt, string WeightText, string GraceDaysText, string LeaveDaysText, string StatusLabel)
{
    public static AssignmentCheckpointView ToAssignmentCheckpointView(AssignmentCheckpoint item)
    {
        return new AssignmentCheckpointView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentCheckpointView> ToAssignmentCheckpointViews(List<AssignmentCheckpoint> items)
    {
        var views = new List<AssignmentCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentCheckpointView(item));
        }
        return views;
    }

    public static double SumAssignmentCheckpointView(AssignmentCheckpointView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedVersion + (double)parsedStartsAt;
    }
}
