namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile manifest.</summary>
public sealed record SkillProfileManifestView(string Code, string Label, string TeamText, string Ratio, string Weight, string OwnerId, string Name, string RevisionText, string Limit, string Country, string StatusLabel)
{
    public static SkillProfileManifestView ToSkillProfileManifestView(SkillProfileManifest item)
    {
        return new SkillProfileManifestView(
            item.Code,
            item.Label,
            item.Team,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileManifestView> ToSkillProfileManifestViews(List<SkillProfileManifest> items)
    {
        var views = new List<SkillProfileManifestView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileManifestView(item));
        }
        return views;
    }

    public static double SumSkillProfileManifestView(SkillProfileManifestView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedWeight + (double)parsedRevision;
    }
}
