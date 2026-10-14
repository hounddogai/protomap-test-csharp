namespace Acme.Workforce.Workplace.Desk.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk manifest.</summary>
public sealed record DeskManifestView(string Code, string NameText, string Reviewer, string Revision, string UnitText, string StatusLabel)
{
    public static DeskManifestView ToDeskManifestView(DeskManifest item)
    {
        return new DeskManifestView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.Reviewer, 40),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskManifestView> ToDeskManifestViews(List<DeskManifest> items)
    {
        var views = new List<DeskManifestView>();
        foreach (var item in items)
        {
            views.Add(ToDeskManifestView(item));
        }
        return views;
    }

    public static double SumDeskManifestView(DeskManifestView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
