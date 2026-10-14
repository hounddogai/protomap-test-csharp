namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool score.</summary>
public sealed record BonusPoolScoreView(string Code, string Title, string Sequence, string UpdatedAt, string StatusLabel)
{
    public static BonusPoolScoreView ToBonusPoolScoreView(BonusPoolScore item)
    {
        return new BonusPoolScoreView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolScoreView> ToBonusPoolScoreViews(List<BonusPoolScore> items)
    {
        var views = new List<BonusPoolScoreView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolScoreView(item));
        }
        return views;
    }

    public static double SumBonusPoolScoreView(BonusPoolScoreView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedUpdatedAt;
    }
}
