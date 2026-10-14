namespace Acme.Workforce.Compliance.Certification.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification snapshot.</summary>
public sealed record CertificationSnapshotView(string Code, string Label, string Hours, string Source, string Amount, string Discount, string StatusLabel)
{
    public static CertificationSnapshotView ToCertificationSnapshotView(CertificationSnapshot item)
    {
        return new CertificationSnapshotView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationSnapshotView> ToCertificationSnapshotViews(List<CertificationSnapshot> items)
    {
        var views = new List<CertificationSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationSnapshotView(item));
        }
        return views;
    }

    public static double SumCertificationSnapshotView(CertificationSnapshotView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedHours + parsedAmount + parsedDiscount;
    }
}
