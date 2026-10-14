namespace Acme.Workforce.Absence.Balance.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance target.</summary>
public sealed record BalanceTargetView(string Code, string Name, string Timezone, string Title, string JobCodeText, string UnitText, string QuantityText, string StatusLabel)
{
    public static BalanceTargetView ToBalanceTargetView(BalanceTarget item)
    {
        return new BalanceTargetView(
            item.Code,
            item.Name,
            item.Timezone,
            item.Title,
            Text.Truncate(item.JobCode, 64),
            item.Unit,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceTargetView> ToBalanceTargetViews(List<BalanceTarget> items)
    {
        var views = new List<BalanceTargetView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceTargetView(item));
        }
        return views;
    }

    public static double SumBalanceTargetView(BalanceTargetView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
