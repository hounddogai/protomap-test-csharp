namespace Acme.Workforce.Workplace.Desk.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk template.</summary>
public sealed record DeskTemplateView(string Code, string LabelText, string CountryText, string Retries, string Description, string Margin, string Region, string CurrencyText, string Name, string Ratio, string TenureMonthsText, string StatusLabel)
{
    public static DeskTemplateView ToDeskTemplateView(DeskTemplate item)
    {
        return new DeskTemplateView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Country,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            Text.Truncate(item.Currency, 80),
            Text.Truncate(item.Name, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskTemplateView> ToDeskTemplateViews(List<DeskTemplate> items)
    {
        var views = new List<DeskTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToDeskTemplateView(item));
        }
        return views;
    }

    public static double SumDeskTemplateView(DeskTemplateView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedMargin + parsedRatio;
    }
}
