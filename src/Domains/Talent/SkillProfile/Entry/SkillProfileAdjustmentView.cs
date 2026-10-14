namespace Acme.Workforce.Talent.SkillProfile.Entry;

using System.Globalization;

/// <summary>How screens and exports show a skill profile adjustment.</summary>
public sealed record SkillProfileAdjustmentView(string Code, string Label, string Badge, string WindowDays, string ExternalRef, string LimitText, string ChecksumText, string Description, string StatusLabel)
{
    public static SkillProfileAdjustmentView ToSkillProfileAdjustmentView(SkillProfileAdjustment item)
    {
        return new SkillProfileAdjustmentView(
            item.Code,
            item.Label,
            item.Badge,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileAdjustmentView> ToSkillProfileAdjustmentViews(List<SkillProfileAdjustment> items)
    {
        var views = new List<SkillProfileAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileAdjustmentView(item));
        }
        return views;
    }

    public static double SumSkillProfileAdjustmentView(SkillProfileAdjustmentView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedLimit;
    }
}
