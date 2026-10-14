namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using System.Globalization;

/// <summary>How screens and exports show a regional garnishment agreement.</summary>
public sealed record RegionalGarnishmentAgreementView(string Code, string Label, string MaximumText, string Badge, string Grade, string BatchSizeText, string Pinned, string QuantityText, string Approved, string StatusLabel)
{
    public static RegionalGarnishmentAgreementView ToRegionalGarnishmentAgreementView(RegionalGarnishmentAgreement item)
    {
        return new RegionalGarnishmentAgreementView(
            item.Code,
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Grade,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalGarnishmentAgreementView> ToRegionalGarnishmentAgreementViews(List<RegionalGarnishmentAgreement> items)
    {
        var views = new List<RegionalGarnishmentAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalGarnishmentAgreementView(item));
        }
        return views;
    }

    public static double SumRegionalGarnishmentAgreementView(RegionalGarnishmentAgreementView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedBatchSize + (double)parsedQuantity;
    }
}
