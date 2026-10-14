namespace Acme.Workforce.Payrollops.PayRun.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional pay run window.</summary>
public sealed record RegionalPayRunWindowView(string CodeText, string LabelText, string ExpiresAtText, string QuantityText, string LocaleText, string StatusLabel)
{
    public static RegionalPayRunWindowView ToRegionalPayRunWindowView(RegionalPayRunWindow item)
    {
        return new RegionalPayRunWindowView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPayRunWindowView> ToRegionalPayRunWindowViews(List<RegionalPayRunWindow> items)
    {
        var views = new List<RegionalPayRunWindowView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPayRunWindowView(item));
        }
        return views;
    }

    public static double SumRegionalPayRunWindowView(RegionalPayRunWindowView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedQuantity;
    }
}
