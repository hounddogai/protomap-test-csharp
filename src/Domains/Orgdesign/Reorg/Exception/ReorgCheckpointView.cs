namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg checkpoint.</summary>
public sealed record ReorgCheckpointView(string Code, string NameText, string HoursText, string SkillText, string Approved, string SourceText, string StatusLabel)
{
    public static ReorgCheckpointView ToReorgCheckpointView(ReorgCheckpoint item)
    {
        return new ReorgCheckpointView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Approved}",
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgCheckpointView> ToReorgCheckpointViews(List<ReorgCheckpoint> items)
    {
        var views = new List<ReorgCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToReorgCheckpointView(item));
        }
        return views;
    }

    public static double SumReorgCheckpointView(ReorgCheckpointView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedHours;
    }
}
