namespace Acme.Workforce.Compliance.Attestation.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending attestation cycle.</summary>
public sealed record PendingAttestationCycleView(string Code, string Name, string TenureMonths, string Maximum, string Description, string GradeText, string StatusLabel)
{
    public static PendingAttestationCycleView ToPendingAttestationCycleView(PendingAttestationCycle item)
    {
        return new PendingAttestationCycleView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Grade, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAttestationCycleView> ToPendingAttestationCycleViews(List<PendingAttestationCycle> items)
    {
        var views = new List<PendingAttestationCycleView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAttestationCycleView(item));
        }
        return views;
    }

    public static double SumPendingAttestationCycleView(PendingAttestationCycleView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedMaximum;
    }
}
