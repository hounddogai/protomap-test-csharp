namespace Acme.Workforce.Absence.Accrual.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual allocation.</summary>
public sealed record AccrualAllocationView(string Code, string NameText, string Unit, string Location, string ManagerId, string BadgeText, string SalaryBand, string OwnerId, string Amount, string StatusLabel)
{
    public static AccrualAllocationView ToAccrualAllocationView(AccrualAllocation item)
    {
        return new AccrualAllocationView(
            item.Code,
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.Unit, 40),
            item.Location,
            item.ManagerId,
            item.Badge,
            item.SalaryBand,
            item.OwnerId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualAllocationView> ToAccrualAllocationViews(List<AccrualAllocation> items)
    {
        var views = new List<AccrualAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualAllocationView(item));
        }
        return views;
    }

    public static double SumAccrualAllocationView(AccrualAllocationView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
