namespace Acme.Workforce.Absence.Balance.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped balance request.</summary>
public sealed record CappedBalanceRequestView(string Code, string Title, string MaximumText, string Quantity, string Channel, string Description, string Grade, string FteText, string LocaleText, string Rate, string StatusLabel)
{
    public static CappedBalanceRequestView ToCappedBalanceRequestView(CappedBalanceRequest item)
    {
        return new CappedBalanceRequestView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Description,
            item.Grade,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedBalanceRequestView> ToCappedBalanceRequestViews(List<CappedBalanceRequest> items)
    {
        var views = new List<CappedBalanceRequestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedBalanceRequestView(item));
        }
        return views;
    }

    public static double SumCappedBalanceRequestView(CappedBalanceRequestView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedQuantity + parsedFte;
    }
}
