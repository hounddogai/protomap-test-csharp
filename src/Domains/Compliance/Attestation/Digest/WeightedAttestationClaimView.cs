namespace Acme.Workforce.Compliance.Attestation.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted attestation claim.</summary>
public sealed record WeightedAttestationClaimView(string Code, string Title, string ManagerId, string SkillText, string ReviewerText, string Score, string StatusLabel)
{
    public static WeightedAttestationClaimView ToWeightedAttestationClaimView(WeightedAttestationClaim item)
    {
        return new WeightedAttestationClaimView(
            item.Code,
            item.Title,
            item.ManagerId,
            Text.Truncate(item.Skill, 120),
            item.Reviewer,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAttestationClaimView> ToWeightedAttestationClaimViews(List<WeightedAttestationClaim> items)
    {
        var views = new List<WeightedAttestationClaimView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAttestationClaimView(item));
        }
        return views;
    }

    public static double SumWeightedAttestationClaimView(WeightedAttestationClaimView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
