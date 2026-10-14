namespace Acme.Workforce.Assets.AssetReturn.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered asset return budget.</summary>
public sealed record TieredAssetReturnBudgetView(string Code, string TitleText, string DepartmentText, string EndsAt, string Sequence, string HoursText, string StatusLabel)
{
    public static TieredAssetReturnBudgetView ToTieredAssetReturnBudgetView(TieredAssetReturnBudget item)
    {
        return new TieredAssetReturnBudgetView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Department,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredAssetReturnBudgetView> ToTieredAssetReturnBudgetViews(List<TieredAssetReturnBudget> items)
    {
        var views = new List<TieredAssetReturnBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToTieredAssetReturnBudgetView(item));
        }
        return views;
    }

    public static double SumTieredAssetReturnBudgetView(TieredAssetReturnBudgetView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedSequence + parsedHours;
    }
}
