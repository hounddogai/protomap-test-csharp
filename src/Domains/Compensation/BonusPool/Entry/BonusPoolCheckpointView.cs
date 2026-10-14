namespace Acme.Workforce.Compensation.BonusPool.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool checkpoint.</summary>
public sealed record BonusPoolCheckpointView(string Code, string Label, string Reviewer, string Notes, string Segment, string Archived, string Title, string Visible, string Phone, string DescriptionText, string Amount, string StatusLabel)
{
    public static BonusPoolCheckpointView ToBonusPoolCheckpointView(BonusPoolCheckpoint item)
    {
        return new BonusPoolCheckpointView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Reviewer,
            item.Notes,
            item.Segment,
            $"{item.Archived}",
            item.Title,
            $"{item.Visible}",
            item.Phone,
            item.Description,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolCheckpointView> ToBonusPoolCheckpointViews(List<BonusPoolCheckpoint> items)
    {
        var views = new List<BonusPoolCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolCheckpointView(item));
        }
        return views;
    }

    public static double SumBonusPoolCheckpointView(BonusPoolCheckpointView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
