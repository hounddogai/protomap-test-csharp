namespace Acme.Workforce.Workplace.Desk.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual desk draft.</summary>
public sealed record ManualDeskDraftView(string Code, string Label, string OwnerId, string ArchivedText, string ChecksumText, string Timezone, string SkillText, string ScoreText, string StatusLabel)
{
    public static ManualDeskDraftView ToManualDeskDraftView(ManualDeskDraft item)
    {
        return new ManualDeskDraftView(
            item.Code,
            item.Label,
            item.OwnerId,
            $"{item.Archived}",
            Text.Truncate(item.Checksum, 64),
            item.Timezone,
            item.Skill,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualDeskDraftView> ToManualDeskDraftViews(List<ManualDeskDraft> items)
    {
        var views = new List<ManualDeskDraftView>();
        foreach (var item in items)
        {
            views.Add(ToManualDeskDraftView(item));
        }
        return views;
    }

    public static double SumManualDeskDraftView(ManualDeskDraftView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
