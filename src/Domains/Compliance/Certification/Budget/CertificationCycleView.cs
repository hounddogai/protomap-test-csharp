namespace Acme.Workforce.Compliance.Certification.Budget;

using System.Globalization;

/// <summary>How screens and exports show a certification cycle.</summary>
public sealed record CertificationCycleView(string Code, string Label, string Capacity, string CreatedAt, string Amount, string ExpiresAt, string Attempts, string StatusLabel)
{
    public static CertificationCycleView ToCertificationCycleView(CertificationCycle item)
    {
        return new CertificationCycleView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationCycleView> ToCertificationCycleViews(List<CertificationCycle> items)
    {
        var views = new List<CertificationCycleView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationCycleView(item));
        }
        return views;
    }

    public static double SumCertificationCycleView(CertificationCycleView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedCreatedAt + parsedAmount;
    }
}
