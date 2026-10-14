namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount rule.</summary>
public sealed record HeadcountRuleView(string Code, string Name, string Team, string Channel, string CostCenter, string FloorValue, string PhoneText, string EmployeeIdText, string StatusLabel)
{
    public static HeadcountRuleView ToHeadcountRuleView(HeadcountRule item)
    {
        return new HeadcountRuleView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Team,
            Text.Truncate(item.Channel, 64),
            item.CostCenter,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountRuleView> ToHeadcountRuleViews(List<HeadcountRule> items)
    {
        var views = new List<HeadcountRuleView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountRuleView(item));
        }
        return views;
    }

    public static double SumHeadcountRuleView(HeadcountRuleView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedFloorValue;
    }
}
