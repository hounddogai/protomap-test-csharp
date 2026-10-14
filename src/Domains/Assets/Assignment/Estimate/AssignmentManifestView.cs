namespace Acme.Workforce.Assets.Assignment.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment manifest.</summary>
public sealed record AssignmentManifestView(string CodeText, string Label, string SalaryBand, string Hours, string NameText, string DepartmentText, string Threshold, string Fte, string StatusLabel)
{
    public static AssignmentManifestView ToAssignmentManifestView(AssignmentManifest item)
    {
        return new AssignmentManifestView(
            item.Code,
            item.Label,
            item.SalaryBand,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            Text.Truncate(item.Department, 120),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentManifestView> ToAssignmentManifestViews(List<AssignmentManifest> items)
    {
        var views = new List<AssignmentManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentManifestView(item));
        }
        return views;
    }

    public static double SumAssignmentManifestView(AssignmentManifestView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedHours + parsedThreshold + parsedFte;
    }
}
