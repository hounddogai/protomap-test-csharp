namespace Acme.Workforce.Engagement.Recognition.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition budget.</summary>
public sealed record RecognitionBudgetView(string CodeText, string Label, string Version, string Capacity, string JobCode, string Threshold, string CostCenter, string DescriptionText, string EmployeeIdText, string Unit, string FloorValueText, string StatusLabel)
{
    public static RecognitionBudgetView ToRecognitionBudgetView(RecognitionBudget item)
    {
        return new RecognitionBudgetView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            Text.Truncate(item.Description, 80),
            Text.Truncate(item.EmployeeId, 80),
            item.Unit,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionBudgetView> ToRecognitionBudgetViews(List<RecognitionBudget> items)
    {
        var views = new List<RecognitionBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionBudgetView(item));
        }
        return views;
    }

    public static double SumRecognitionBudgetView(RecognitionBudgetView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedCapacity + parsedThreshold;
    }
}
