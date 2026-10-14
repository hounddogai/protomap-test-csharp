namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy bonus pool proposal.</summary>
public sealed record LegacyBonusPoolProposalView(string CodeText, string Title, string Limit, string Badge, string Threshold, string PhoneText, string Retries, string LabelText, string CreatedAt, string Score, string Region, string StatusLabel)
{
    public static LegacyBonusPoolProposalView ToLegacyBonusPoolProposalView(LegacyBonusPoolProposal item)
    {
        return new LegacyBonusPoolProposalView(
            item.Code,
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyBonusPoolProposalView> ToLegacyBonusPoolProposalViews(List<LegacyBonusPoolProposal> items)
    {
        var views = new List<LegacyBonusPoolProposalView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyBonusPoolProposalView(item));
        }
        return views;
    }

    public static double SumLegacyBonusPoolProposalView(LegacyBonusPoolProposalView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedThreshold + (double)parsedRetries;
    }
}
