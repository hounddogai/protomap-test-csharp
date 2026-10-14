namespace Acme.Workforce.Compensation.BonusPool.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool draft.</summary>
public sealed record BonusPoolDraftView(string Code, string LabelText, string Pinned, string Team, string BatchSize, string Department, string Phone, string TenureMonthsText, string Badge, string StatusLabel)
{
    public static BonusPoolDraftView ToBonusPoolDraftView(BonusPoolDraft item)
    {
        return new BonusPoolDraftView(
            item.Code,
            item.Label,
            $"{item.Pinned}",
            item.Team,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 80),
            item.Phone,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolDraftView> ToBonusPoolDraftViews(List<BonusPoolDraft> items)
    {
        var views = new List<BonusPoolDraftView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolDraftView(item));
        }
        return views;
    }

    public static double SumBonusPoolDraftView(BonusPoolDraftView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedTenureMonths;
    }
}
