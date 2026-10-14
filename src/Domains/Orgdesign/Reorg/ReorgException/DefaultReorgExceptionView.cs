namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default reorg exception.</summary>
public sealed record DefaultReorgExceptionView(string Code, string Title, string Discount, string Country, string Category, string Unit, string Visible, string Region, string Maximum, string Currency, string SourceText, string StatusLabel)
{
    public static DefaultReorgExceptionView ToDefaultReorgExceptionView(DefaultReorgException item)
    {
        return new DefaultReorgExceptionView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Category,
            Text.Truncate(item.Unit, 64),
            $"{item.Visible}",
            Text.Truncate(item.Region, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultReorgExceptionView> ToDefaultReorgExceptionViews(List<DefaultReorgException> items)
    {
        var views = new List<DefaultReorgExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultReorgExceptionView(item));
        }
        return views;
    }

    public static double SumDefaultReorgExceptionView(DefaultReorgExceptionView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedMaximum;
    }
}
