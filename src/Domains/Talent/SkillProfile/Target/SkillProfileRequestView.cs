namespace Acme.Workforce.Talent.SkillProfile.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile request.</summary>
public sealed record SkillProfileRequestView(string CodeText, string Name, string Attempts, string FloorValueText, string Grade, string Badge, string ExpiresAtText, string Skill, string StatusLabel)
{
    public static SkillProfileRequestView ToSkillProfileRequestView(SkillProfileRequest item)
    {
        return new SkillProfileRequestView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            Text.Truncate(item.Badge, 40),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileRequestView> ToSkillProfileRequestViews(List<SkillProfileRequest> items)
    {
        var views = new List<SkillProfileRequestView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileRequestView(item));
        }
        return views;
    }

    public static double SumSkillProfileRequestView(SkillProfileRequestView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedFloorValue + (double)parsedExpiresAt;
    }
}
