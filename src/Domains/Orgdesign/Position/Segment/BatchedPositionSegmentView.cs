namespace Acme.Workforce.Orgdesign.Position.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched position segment.</summary>
public sealed record BatchedPositionSegmentView(string Code, string TitleText, string Category, string AttemptsText, string Reviewer, string CostCenterText, string PhoneText, string BudgetText, string Capacity, string Grade, string Name, string StatusLabel)
{
    public static BatchedPositionSegmentView ToBatchedPositionSegmentView(BatchedPositionSegment item)
    {
        return new BatchedPositionSegmentView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Category,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 64),
            item.CostCenter,
            item.Phone,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            Text.Truncate(item.Name, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPositionSegmentView> ToBatchedPositionSegmentViews(List<BatchedPositionSegment> items)
    {
        var views = new List<BatchedPositionSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPositionSegmentView(item));
        }
        return views;
    }

    public static double SumBatchedPositionSegmentView(BatchedPositionSegmentView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedBudget + (double)parsedCapacity;
    }
}
