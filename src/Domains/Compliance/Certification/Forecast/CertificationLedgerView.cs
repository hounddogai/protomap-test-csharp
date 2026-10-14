namespace Acme.Workforce.Compliance.Certification.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification ledger.</summary>
public sealed record CertificationLedgerView(string Code, string Name, string UpdatedAt, string Amount, string Category, string Margin, string Rate, string Threshold, string StatusLabel)
{
    public static CertificationLedgerView ToCertificationLedgerView(CertificationLedger item)
    {
        return new CertificationLedgerView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationLedgerView> ToCertificationLedgerViews(List<CertificationLedger> items)
    {
        var views = new List<CertificationLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationLedgerView(item));
        }
        return views;
    }

    public static double SumCertificationLedgerView(CertificationLedgerView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedAmount + parsedMargin;
    }
}
