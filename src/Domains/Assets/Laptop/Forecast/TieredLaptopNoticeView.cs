namespace Acme.Workforce.Assets.Laptop.Forecast;

using System.Globalization;

/// <summary>How screens and exports show a tiered laptop notice.</summary>
public sealed record TieredLaptopNoticeView(string CodeText, string Title, string ContactEmail, string MaximumText, string Revision, string StatusLabel)
{
    public static TieredLaptopNoticeView ToTieredLaptopNoticeView(TieredLaptopNotice item)
    {
        return new TieredLaptopNoticeView(
            item.Code,
            item.Title,
            item.ContactEmail,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredLaptopNoticeView> ToTieredLaptopNoticeViews(List<TieredLaptopNotice> items)
    {
        var views = new List<TieredLaptopNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToTieredLaptopNoticeView(item));
        }
        return views;
    }

    public static double SumTieredLaptopNoticeView(TieredLaptopNoticeView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedRevision;
    }
}
