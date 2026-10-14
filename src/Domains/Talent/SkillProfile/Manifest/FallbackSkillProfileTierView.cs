namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using System.Globalization;

/// <summary>How screens and exports show a fallback skill profile tier.</summary>
public sealed record FallbackSkillProfileTierView(string CodeText, string Name, string Pinned, string Version, string Grade, string Ceiling, string StatusLabel)
{
    public static FallbackSkillProfileTierView ToFallbackSkillProfileTierView(FallbackSkillProfileTier item)
    {
        return new FallbackSkillProfileTierView(
            item.Code,
            item.Name,
            $"{item.Pinned}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackSkillProfileTierView> ToFallbackSkillProfileTierViews(List<FallbackSkillProfileTier> items)
    {
        var views = new List<FallbackSkillProfileTierView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackSkillProfileTierView(item));
        }
        return views;
    }

    public static double SumFallbackSkillProfileTierView(FallbackSkillProfileTierView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedCeiling;
    }
}
