namespace Acme.Workforce.Absence.Accrual.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default accrual manifest.</summary>
public sealed record DefaultAccrualManifestView(string Code, string Title, string WindowDays, string Sequence, string Location, string Timezone, string CreatedAtText, string Badge, string Unit, string DepartmentText, string StatusLabel)
{
    public static DefaultAccrualManifestView ToDefaultAccrualManifestView(DefaultAccrualManifest item)
    {
        return new DefaultAccrualManifestView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Timezone,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Unit,
            Text.Truncate(item.Department, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAccrualManifestView> ToDefaultAccrualManifestViews(List<DefaultAccrualManifest> items)
    {
        var views = new List<DefaultAccrualManifestView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAccrualManifestView(item));
        }
        return views;
    }

    public static double SumDefaultAccrualManifestView(DefaultAccrualManifestView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedSequence + (double)parsedCreatedAt;
    }
}
