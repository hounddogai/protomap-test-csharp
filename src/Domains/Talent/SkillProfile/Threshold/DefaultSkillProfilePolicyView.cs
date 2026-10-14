namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a default skill profile policy.</summary>
public sealed record DefaultSkillProfilePolicyView(string Code, string Label, string UpdatedAt, string RatingText, string LeaveDays, string Attempts, string StatusLabel)
{
    public static DefaultSkillProfilePolicyView ToDefaultSkillProfilePolicyView(DefaultSkillProfilePolicy item)
    {
        return new DefaultSkillProfilePolicyView(
            item.Code,
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultSkillProfilePolicyView> ToDefaultSkillProfilePolicyViews(List<DefaultSkillProfilePolicy> items)
    {
        var views = new List<DefaultSkillProfilePolicyView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultSkillProfilePolicyView(item));
        }
        return views;
    }

    public static double SumDefaultSkillProfilePolicyView(DefaultSkillProfilePolicyView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRating + (double)parsedLeaveDays;
    }
}
