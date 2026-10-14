namespace Acme.Workforce.Compensation.BonusPool.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool profile.</summary>
public sealed record BonusPoolProfileView(string Code, string NameText, string Budget, string Title, string SalaryBand, string StatusLabel)
{
    public static BonusPoolProfileView ToBonusPoolProfileView(BonusPoolProfile item)
    {
        return new BonusPoolProfileView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 40),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            Text.Truncate(item.SalaryBand, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolProfileView> ToBonusPoolProfileViews(List<BonusPoolProfile> items)
    {
        var views = new List<BonusPoolProfileView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolProfileView(item));
        }
        return views;
    }

    public static double SumBonusPoolProfileView(BonusPoolProfileView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
