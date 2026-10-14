namespace Acme.Workforce.Compensation.BonusPool.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted bonus pool quota.</summary>
public sealed record WeightedBonusPoolQuotaView(string Code, string Name, string Checksum, string Description, string Channel, string TenureMonths, string Team, string GraceDays, string Rating, string Limit, string StatusLabel)
{
    public static WeightedBonusPoolQuotaView ToWeightedBonusPoolQuotaView(WeightedBonusPoolQuota item)
    {
        return new WeightedBonusPoolQuotaView(
            item.Code,
            item.Name,
            Text.Truncate(item.Checksum, 64),
            item.Description,
            item.Channel,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedBonusPoolQuotaView> ToWeightedBonusPoolQuotaViews(List<WeightedBonusPoolQuota> items)
    {
        var views = new List<WeightedBonusPoolQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedBonusPoolQuotaView(item));
        }
        return views;
    }

    public static double SumWeightedBonusPoolQuotaView(WeightedBonusPoolQuotaView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedGraceDays + parsedRating;
    }
}
