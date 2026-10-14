namespace Acme.Workforce.Workplace.Visitor.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor manifest.</summary>
public sealed record VisitorManifestView(string CodeText, string NameText, string Locale, string Reviewer, string Approved, string Timezone, string HoursText, string ManagerIdText, string Budget, string EmployeeId, string TenureMonths, string StatusLabel)
{
    public static VisitorManifestView ToVisitorManifestView(VisitorManifest item)
    {
        return new VisitorManifestView(
            item.Code,
            item.Name,
            item.Locale,
            Text.Truncate(item.Reviewer, 120),
            $"{item.Approved}",
            item.Timezone,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorManifestView> ToVisitorManifestViews(List<VisitorManifest> items)
    {
        var views = new List<VisitorManifestView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorManifestView(item));
        }
        return views;
    }

    public static double SumVisitorManifestView(VisitorManifestView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedHours + parsedBudget + (double)parsedTenureMonths;
    }
}
