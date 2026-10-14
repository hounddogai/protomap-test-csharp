namespace Acme.Workforce.Payrollops.TaxForm.Template;

using System.Globalization;

/// <summary>How screens and exports show a tax form rollup.</summary>
public sealed record TaxFormRollupView(string Code, string Title, string ExternalRef, string RatioText, string Revision, string PinnedText, string GraceDays, string StatusLabel)
{
    public static TaxFormRollupView ToTaxFormRollupView(TaxFormRollup item)
    {
        return new TaxFormRollupView(
            item.Code,
            item.Title,
            item.ExternalRef,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormRollupView> ToTaxFormRollupViews(List<TaxFormRollup> items)
    {
        var views = new List<TaxFormRollupView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormRollupView(item));
        }
        return views;
    }

    public static double SumTaxFormRollupView(TaxFormRollupView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedRevision + (double)parsedGraceDays;
    }
}
