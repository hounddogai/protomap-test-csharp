namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using System.Globalization;

/// <summary>How screens and exports show a lenient tax form notice.</summary>
public sealed record LenientTaxFormNoticeView(string Code, string Name, string Discount, string Skill, string Archived, string Hours, string Ratio, string StatusLabel)
{
    public static LenientTaxFormNoticeView ToLenientTaxFormNoticeView(LenientTaxFormNotice item)
    {
        return new LenientTaxFormNoticeView(
            item.Code,
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Archived}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientTaxFormNoticeView> ToLenientTaxFormNoticeViews(List<LenientTaxFormNotice> items)
    {
        var views = new List<LenientTaxFormNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToLenientTaxFormNoticeView(item));
        }
        return views;
    }

    public static double SumLenientTaxFormNoticeView(LenientTaxFormNoticeView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedHours + parsedRatio;
    }
}
