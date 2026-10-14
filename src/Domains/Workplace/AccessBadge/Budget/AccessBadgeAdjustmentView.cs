namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge adjustment.</summary>
public sealed record AccessBadgeAdjustmentView(string Code, string Title, string CountryText, string Score, string ExpiresAt, string StatusLabel)
{
    public static AccessBadgeAdjustmentView ToAccessBadgeAdjustmentView(AccessBadgeAdjustment item)
    {
        return new AccessBadgeAdjustmentView(
            Text.Truncate(item.Code, 40),
            item.Title,
            Text.Truncate(item.Country, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeAdjustmentView> ToAccessBadgeAdjustmentViews(List<AccessBadgeAdjustment> items)
    {
        var views = new List<AccessBadgeAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeAdjustmentView(item));
        }
        return views;
    }

    public static double SumAccessBadgeAdjustmentView(AccessBadgeAdjustmentView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedExpiresAt;
    }
}
