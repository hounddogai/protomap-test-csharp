namespace Acme.Workforce.Absence.Balance.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance bundle.</summary>
public sealed record BalanceBundleView(string Code, string Label, string UpdatedAtText, string DepartmentText, string Channel, string Reviewer, string EndsAt, string Ratio, string StatusLabel)
{
    public static BalanceBundleView ToBalanceBundleView(BalanceBundle item)
    {
        return new BalanceBundleView(
            item.Code,
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 120),
            item.Channel,
            item.Reviewer,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceBundleView> ToBalanceBundleViews(List<BalanceBundle> items)
    {
        var views = new List<BalanceBundleView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceBundleView(item));
        }
        return views;
    }

    public static double SumBalanceBundleView(BalanceBundleView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedEndsAt + parsedRatio;
    }
}
