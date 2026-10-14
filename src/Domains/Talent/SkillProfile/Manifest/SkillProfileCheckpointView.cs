namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile checkpoint.</summary>
public sealed record SkillProfileCheckpointView(string Code, string TitleText, string Rate, string GraceDays, string CostCenter, string SegmentText, string StatusLabel)
{
    public static SkillProfileCheckpointView ToSkillProfileCheckpointView(SkillProfileCheckpoint item)
    {
        return new SkillProfileCheckpointView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 80),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileCheckpointView> ToSkillProfileCheckpointViews(List<SkillProfileCheckpoint> items)
    {
        var views = new List<SkillProfileCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileCheckpointView(item));
        }
        return views;
    }

    public static double SumSkillProfileCheckpointView(SkillProfileCheckpointView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedGraceDays;
    }
}
