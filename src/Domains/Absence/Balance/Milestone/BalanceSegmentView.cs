namespace Acme.Workforce.Absence.Balance.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance segment.</summary>
public sealed record BalanceSegmentView(string CodeText, string Title, string UpdatedAt, string CreatedAt, string BudgetText, string Weight, string Discount, string StatusLabel)
{
    public static BalanceSegmentView ToBalanceSegmentView(BalanceSegment item)
    {
        return new BalanceSegmentView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceSegmentView> ToBalanceSegmentViews(List<BalanceSegment> items)
    {
        var views = new List<BalanceSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceSegmentView(item));
        }
        return views;
    }

    public static double SumBalanceSegmentView(BalanceSegmentView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedCreatedAt + parsedBudget;
    }
}
