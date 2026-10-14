namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched skill profile notice.</summary>
public sealed record BatchedSkillProfileNoticeView(string Code, string Name, string FloorValue, string Skill, string Label, string CountryText, string Margin, string PinnedText, string StatusLabel)
{
    public static BatchedSkillProfileNoticeView ToBatchedSkillProfileNoticeView(BatchedSkillProfileNotice item)
    {
        return new BatchedSkillProfileNoticeView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Label,
            Text.Truncate(item.Country, 64),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedSkillProfileNoticeView> ToBatchedSkillProfileNoticeViews(List<BatchedSkillProfileNotice> items)
    {
        var views = new List<BatchedSkillProfileNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedSkillProfileNoticeView(item));
        }
        return views;
    }

    public static double SumBatchedSkillProfileNoticeView(BatchedSkillProfileNoticeView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMargin;
    }
}
