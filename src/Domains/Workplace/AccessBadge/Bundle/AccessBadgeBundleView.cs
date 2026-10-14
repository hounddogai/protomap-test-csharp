namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge bundle.</summary>
public sealed record AccessBadgeBundleView(string Code, string Name, string Grade, string CostText, string ManagerId, string Discount, string SequenceText, string Notes, string GraceDays, string Description, string StatusLabel)
{
    public static AccessBadgeBundleView ToAccessBadgeBundleView(AccessBadgeBundle item)
    {
        return new AccessBadgeBundleView(
            item.Code,
            item.Name,
            Text.Truncate(item.Grade, 40),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeBundleView> ToAccessBadgeBundleViews(List<AccessBadgeBundle> items)
    {
        var views = new List<AccessBadgeBundleView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeBundleView(item));
        }
        return views;
    }

    public static double SumAccessBadgeBundleView(AccessBadgeBundleView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return parsedCost + parsedDiscount + (double)parsedSequence;
    }
}
