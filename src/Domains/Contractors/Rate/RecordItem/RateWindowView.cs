namespace Acme.Workforce.Contractors.Rate.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate window.</summary>
public sealed record RateWindowView(string Code, string Title, string ExpiresAt, string Amount, string CapacityText, string StatusLabel)
{
    public static RateWindowView ToRateWindowView(RateWindow item)
    {
        return new RateWindowView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateWindowView> ToRateWindowViews(List<RateWindow> items)
    {
        var views = new List<RateWindowView>();
        foreach (var item in items)
        {
            views.Add(ToRateWindowView(item));
        }
        return views;
    }

    public static double SumRateWindowView(RateWindowView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedAmount + (double)parsedCapacity;
    }
}
