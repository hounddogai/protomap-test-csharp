namespace Acme.Workforce.Talent.Pool.PoolWindow;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled pool window.</summary>
public sealed record ScheduledPoolWindowView(string Code, string Title, string Category, string MaximumText, string CurrencyText, string Weight, string EmployeeId, string CountryText, string Checksum, string StatusLabel)
{
    public static ScheduledPoolWindowView ToScheduledPoolWindowView(ScheduledPoolWindow item)
    {
        return new ScheduledPoolWindowView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Category,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Country,
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPoolWindowView> ToScheduledPoolWindowViews(List<ScheduledPoolWindow> items)
    {
        var views = new List<ScheduledPoolWindowView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPoolWindowView(item));
        }
        return views;
    }

    public static double SumScheduledPoolWindowView(ScheduledPoolWindowView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedWeight;
    }
}
