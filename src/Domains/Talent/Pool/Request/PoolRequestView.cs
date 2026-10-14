namespace Acme.Workforce.Talent.Pool.Request;

using System.Globalization;

/// <summary>How screens and exports show a pool request.</summary>
public sealed record PoolRequestView(string Code, string NameText, string CapacityText, string CurrencyText, string CostCenterText, string Discount, string ContactEmail, string CostText, string StatusLabel)
{
    public static PoolRequestView ToPoolRequestView(PoolRequest item)
    {
        return new PoolRequestView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.CostCenter,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolRequestView> ToPoolRequestViews(List<PoolRequest> items)
    {
        var views = new List<PoolRequestView>();
        foreach (var item in items)
        {
            views.Add(ToPoolRequestView(item));
        }
        return views;
    }

    public static double SumPoolRequestView(PoolRequestView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedDiscount + parsedCost;
    }
}
