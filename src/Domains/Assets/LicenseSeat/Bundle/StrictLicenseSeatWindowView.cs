namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict license seat window.</summary>
public sealed record StrictLicenseSeatWindowView(string Code, string Label, string Limit, string UnitText, string Cost, string EmployeeIdText, string StatusLabel)
{
    public static StrictLicenseSeatWindowView ToStrictLicenseSeatWindowView(StrictLicenseSeatWindow item)
    {
        return new StrictLicenseSeatWindowView(
            item.Code,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictLicenseSeatWindowView> ToStrictLicenseSeatWindowViews(List<StrictLicenseSeatWindow> items)
    {
        var views = new List<StrictLicenseSeatWindowView>();
        foreach (var item in items)
        {
            views.Add(ToStrictLicenseSeatWindowView(item));
        }
        return views;
    }

    public static double SumStrictLicenseSeatWindowView(StrictLicenseSeatWindowView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedCost;
    }
}
