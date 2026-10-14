namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle manifest.</summary>
public sealed record MeritCycleManifestView(string Code, string Title, string Discount, string NotesText, string CostCenterText, string NameText, string EmployeeId, string UpdatedAt, string Revision, string Priority, string StatusLabel)
{
    public static MeritCycleManifestView ToMeritCycleManifestView(MeritCycleManifest item)
    {
        return new MeritCycleManifestView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.CostCenter, 120),
            item.Name,
            item.EmployeeId,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleManifestView> ToMeritCycleManifestViews(List<MeritCycleManifest> items)
    {
        var views = new List<MeritCycleManifestView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleManifestView(item));
        }
        return views;
    }

    public static double SumMeritCycleManifestView(MeritCycleManifestView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedUpdatedAt + (double)parsedRevision;
    }
}
