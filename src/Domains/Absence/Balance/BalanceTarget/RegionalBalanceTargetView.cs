namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional balance target.</summary>
public sealed record RegionalBalanceTargetView(string Code, string NameText, string DurationMinutes, string Pinned, string Phone, string Active, string Unit, string Budget, string StatusLabel)
{
    public static RegionalBalanceTargetView ToRegionalBalanceTargetView(RegionalBalanceTarget item)
    {
        return new RegionalBalanceTargetView(
            item.Code,
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Phone, 80),
            $"{item.Active}",
            item.Unit,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalBalanceTargetView> ToRegionalBalanceTargetViews(List<RegionalBalanceTarget> items)
    {
        var views = new List<RegionalBalanceTargetView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalBalanceTargetView(item));
        }
        return views;
    }

    public static double SumRegionalBalanceTargetView(RegionalBalanceTargetView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedBudget;
    }
}
