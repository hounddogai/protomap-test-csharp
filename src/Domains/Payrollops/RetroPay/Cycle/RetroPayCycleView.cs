namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay cycle.</summary>
public sealed record RetroPayCycleView(string CodeText, string NameText, string CurrencyText, string ExpiresAtText, string Notes, string LimitText, string DiscountText, string StatusLabel)
{
    public static RetroPayCycleView ToRetroPayCycleView(RetroPayCycle item)
    {
        return new RetroPayCycleView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Currency,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayCycleView> ToRetroPayCycleViews(List<RetroPayCycle> items)
    {
        var views = new List<RetroPayCycleView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayCycleView(item));
        }
        return views;
    }

    public static double SumRetroPayCycleView(RetroPayCycleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedLimit + parsedDiscount;
    }
}
