namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile bundle.</summary>
public sealed record SkillProfileBundleView(string Code, string Label, string TimezoneText, string ExternalRef, string MarginText, string StatusLabel)
{
    public static SkillProfileBundleView ToSkillProfileBundleView(SkillProfileBundle item)
    {
        return new SkillProfileBundleView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Timezone,
            item.ExternalRef,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileBundleView> ToSkillProfileBundleViews(List<SkillProfileBundle> items)
    {
        var views = new List<SkillProfileBundleView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileBundleView(item));
        }
        return views;
    }

    public static double SumSkillProfileBundleView(SkillProfileBundleView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return parsedMargin;
    }
}
