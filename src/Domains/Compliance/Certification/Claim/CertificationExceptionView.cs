namespace Acme.Workforce.Compliance.Certification.Claim;

using System.Globalization;

/// <summary>How screens and exports show a certification exception.</summary>
public sealed record CertificationExceptionView(string CodeText, string LabelText, string Discount, string ChecksumText, string MarginText, string TenureMonths, string Grade, string Score, string CreatedAt, string UpdatedAtText, string StatusLabel)
{
    public static CertificationExceptionView ToCertificationExceptionView(CertificationException item)
    {
        return new CertificationExceptionView(
            item.Code,
            item.Label,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationExceptionView> ToCertificationExceptionViews(List<CertificationException> items)
    {
        var views = new List<CertificationExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationExceptionView(item));
        }
        return views;
    }

    public static double SumCertificationExceptionView(CertificationExceptionView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedMargin + (double)parsedTenureMonths;
    }
}
