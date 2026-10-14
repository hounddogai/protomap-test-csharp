namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted return plan manifest.</summary>
public sealed record WeightedReturnPlanManifestView(string Code, string NameText, string Maximum, string EmployeeId, string Minimum, string Budget, string StatusLabel)
{
    public static WeightedReturnPlanManifestView ToWeightedReturnPlanManifestView(WeightedReturnPlanManifest item)
    {
        return new WeightedReturnPlanManifestView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedReturnPlanManifestView> ToWeightedReturnPlanManifestViews(List<WeightedReturnPlanManifest> items)
    {
        var views = new List<WeightedReturnPlanManifestView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedReturnPlanManifestView(item));
        }
        return views;
    }

    public static double SumWeightedReturnPlanManifestView(WeightedReturnPlanManifestView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedMinimum + parsedBudget;
    }
}
