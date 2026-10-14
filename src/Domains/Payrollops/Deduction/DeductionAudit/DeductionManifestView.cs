namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using System.Globalization;

/// <summary>How screens and exports show a deduction manifest.</summary>
public sealed record DeductionManifestView(string Code, string Name, string WindowDays, string Visible, string SourceText, string Score, string StatusLabel)
{
    public static DeductionManifestView ToDeductionManifestView(DeductionManifest item)
    {
        return new DeductionManifestView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Source,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionManifestView> ToDeductionManifestViews(List<DeductionManifest> items)
    {
        var views = new List<DeductionManifestView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionManifestView(item));
        }
        return views;
    }

    public static double SumDeductionManifestView(DeductionManifestView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedScore;
    }
}
