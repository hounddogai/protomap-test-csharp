namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile quota.</summary>
public sealed record SkillProfileQuotaView(string Code, string Label, string Checksum, string FteText, string Badge, string Skill, string Amount, string CurrencyText, string StatusLabel)
{
    public static SkillProfileQuotaView ToSkillProfileQuotaView(SkillProfileQuota item)
    {
        return new SkillProfileQuotaView(
            item.Code,
            item.Label,
            Text.Truncate(item.Checksum, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Skill,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileQuotaView> ToSkillProfileQuotaViews(List<SkillProfileQuota> items)
    {
        var views = new List<SkillProfileQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileQuotaView(item));
        }
        return views;
    }

    public static double SumSkillProfileQuotaView(SkillProfileQuotaView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedFte + parsedAmount;
    }
}
