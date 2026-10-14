namespace Acme.Workforce.Compensation.BonusPool.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool digest.</summary>
public sealed record BonusPoolDigestView(string Code, string Name, string ThresholdText, string UpdatedAt, string VersionText, string ApprovedText, string StatusLabel)
{
    public static BonusPoolDigestView ToBonusPoolDigestView(BonusPoolDigest item)
    {
        return new BonusPoolDigestView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolDigestView> ToBonusPoolDigestViews(List<BonusPoolDigest> items)
    {
        var views = new List<BonusPoolDigestView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolDigestView(item));
        }
        return views;
    }

    public static double SumBonusPoolDigestView(BonusPoolDigestView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedUpdatedAt + (double)parsedVersion;
    }
}
