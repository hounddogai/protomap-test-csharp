namespace Acme.Workforce.Contractors.Offboarding.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched offboarding baseline.</summary>
public sealed record BatchedOffboardingBaselineView(string CodeText, string Label, string EndsAt, string Department, string ManagerId, string Locale, string FloorValue, string Category, string NotesText, string StatusLabel)
{
    public static BatchedOffboardingBaselineView ToBatchedOffboardingBaselineView(BatchedOffboardingBaseline item)
    {
        return new BatchedOffboardingBaselineView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 80),
            Text.Truncate(item.ManagerId, 40),
            item.Locale,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedOffboardingBaselineView> ToBatchedOffboardingBaselineViews(List<BatchedOffboardingBaseline> items)
    {
        var views = new List<BatchedOffboardingBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedOffboardingBaselineView(item));
        }
        return views;
    }

    public static double SumBatchedOffboardingBaselineView(BatchedOffboardingBaselineView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedFloorValue;
    }
}
