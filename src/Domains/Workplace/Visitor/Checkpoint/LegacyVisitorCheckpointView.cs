namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy visitor checkpoint.</summary>
public sealed record LegacyVisitorCheckpointView(string CodeText, string TitleText, string DurationMinutesText, string DepartmentText, string RatioText, string ScoreText, string FloorValue, string DescriptionText, string StatusLabel)
{
    public static LegacyVisitorCheckpointView ToLegacyVisitorCheckpointView(LegacyVisitorCheckpoint item)
    {
        return new LegacyVisitorCheckpointView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyVisitorCheckpointView> ToLegacyVisitorCheckpointViews(List<LegacyVisitorCheckpoint> items)
    {
        var views = new List<LegacyVisitorCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyVisitorCheckpointView(item));
        }
        return views;
    }

    public static double SumLegacyVisitorCheckpointView(LegacyVisitorCheckpointView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRatio + parsedScore;
    }
}
