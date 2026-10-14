namespace Acme.Workforce.Payrollops.PayRun.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional pay run group.</summary>
public sealed record RegionalPayRunGroupView(string Code, string Name, string ContactEmail, string Budget, string Maximum, string FloorValue, string Summary, string FteText, string Quantity, string DescriptionText, string StatusLabel)
{
    public static RegionalPayRunGroupView ToRegionalPayRunGroupView(RegionalPayRunGroup item)
    {
        return new RegionalPayRunGroupView(
            item.Code,
            item.Name,
            Text.Truncate(item.ContactEmail, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPayRunGroupView> ToRegionalPayRunGroupViews(List<RegionalPayRunGroup> items)
    {
        var views = new List<RegionalPayRunGroupView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPayRunGroupView(item));
        }
        return views;
    }

    public static double SumRegionalPayRunGroupView(RegionalPayRunGroupView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMaximum + parsedFloorValue;
    }
}
