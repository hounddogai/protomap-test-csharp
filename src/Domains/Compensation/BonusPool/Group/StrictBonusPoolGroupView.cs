namespace Acme.Workforce.Compensation.BonusPool.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict bonus pool group.</summary>
public sealed record StrictBonusPoolGroupView(string CodeText, string Title, string TenureMonths, string OwnerId, string EmployeeId, string Channel, string ManagerId, string StatusLabel)
{
    public static StrictBonusPoolGroupView ToStrictBonusPoolGroupView(StrictBonusPoolGroup item)
    {
        return new StrictBonusPoolGroupView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.EmployeeId,
            Text.Truncate(item.Channel, 80),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictBonusPoolGroupView> ToStrictBonusPoolGroupViews(List<StrictBonusPoolGroup> items)
    {
        var views = new List<StrictBonusPoolGroupView>();
        foreach (var item in items)
        {
            views.Add(ToStrictBonusPoolGroupView(item));
        }
        return views;
    }

    public static double SumStrictBonusPoolGroupView(StrictBonusPoolGroupView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
