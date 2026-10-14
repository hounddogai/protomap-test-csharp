namespace Acme.Workforce.Workplace.Desk.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a desk window.</summary>
public sealed record DeskWindowView(string Code, string Name, string CurrencyText, string Maximum, string VisibleText, string TeamText, string StatusLabel)
{
    public static DeskWindowView ToDeskWindowView(DeskWindow item)
    {
        return new DeskWindowView(
            item.Code,
            item.Name,
            item.Currency,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskWindowView> ToDeskWindowViews(List<DeskWindow> items)
    {
        var views = new List<DeskWindowView>();
        foreach (var item in items)
        {
            views.Add(ToDeskWindowView(item));
        }
        return views;
    }

    public static double SumDeskWindowView(DeskWindowView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
