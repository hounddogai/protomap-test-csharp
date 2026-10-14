namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using System.Globalization;

/// <summary>How screens and exports show a capped laptop record.</summary>
public sealed record CappedLaptopRecordView(string Code, string NameText, string Badge, string Fte, string ExpiresAt, string Discount, string Visible, string LeaveDays, string StatusLabel)
{
    public static CappedLaptopRecordView ToCappedLaptopRecordView(CappedLaptopRecord item)
    {
        return new CappedLaptopRecordView(
            item.Code,
            item.Name,
            item.Badge,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedLaptopRecordView> ToCappedLaptopRecordViews(List<CappedLaptopRecord> items)
    {
        var views = new List<CappedLaptopRecordView>();
        foreach (var item in items)
        {
            views.Add(ToCappedLaptopRecordView(item));
        }
        return views;
    }

    public static double SumCappedLaptopRecordView(CappedLaptopRecordView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedExpiresAt + parsedDiscount;
    }
}
