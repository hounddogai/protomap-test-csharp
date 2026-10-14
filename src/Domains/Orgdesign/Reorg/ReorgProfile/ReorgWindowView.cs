namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using System.Globalization;

/// <summary>How screens and exports show a reorg window.</summary>
public sealed record ReorgWindowView(string CodeText, string Name, string Country, string Unit, string Amount, string StatusLabel)
{
    public static ReorgWindowView ToReorgWindowView(ReorgWindow item)
    {
        return new ReorgWindowView(
            item.Code,
            item.Name,
            item.Country,
            item.Unit,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgWindowView> ToReorgWindowViews(List<ReorgWindow> items)
    {
        var views = new List<ReorgWindowView>();
        foreach (var item in items)
        {
            views.Add(ToReorgWindowView(item));
        }
        return views;
    }

    public static double SumReorgWindowView(ReorgWindowView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
