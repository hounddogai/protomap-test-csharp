namespace Acme.Workforce.Absence.Holiday.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday quota.</summary>
public sealed record HolidayQuotaView(string Code, string NameText, string QuantityText, string ScoreText, string ContactEmail, string SummaryText, string Amount, string Active, string FloorValueText, string StatusLabel)
{
    public static HolidayQuotaView ToHolidayQuotaView(HolidayQuota item)
    {
        return new HolidayQuotaView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Summary,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayQuotaView> ToHolidayQuotaViews(List<HolidayQuota> items)
    {
        var views = new List<HolidayQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayQuotaView(item));
        }
        return views;
    }

    public static double SumHolidayQuotaView(HolidayQuotaView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedScore + parsedAmount;
    }
}
