namespace Acme.Workforce.Absence.Accrual.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual window.</summary>
public sealed record AccrualWindowView(string Code, string Title, string Locale, string Reviewer, string ManagerId, string Margin, string JobCodeText, string QuantityText, string StatusLabel)
{
    public static AccrualWindowView ToAccrualWindowView(AccrualWindow item)
    {
        return new AccrualWindowView(
            item.Code,
            item.Title,
            item.Locale,
            Text.Truncate(item.Reviewer, 40),
            item.ManagerId,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualWindowView> ToAccrualWindowViews(List<AccrualWindow> items)
    {
        var views = new List<AccrualWindowView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualWindowView(item));
        }
        return views;
    }

    public static double SumAccrualWindowView(AccrualWindowView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedQuantity;
    }
}
