namespace Acme.Workforce.Engagement.Recognition.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional recognition snapshot.</summary>
public sealed record RegionalRecognitionSnapshotView(string CodeText, string Title, string BudgetText, string Amount, string Category, string EmployeeId, string JobCodeText, string Phone, string StatusLabel)
{
    public static RegionalRecognitionSnapshotView ToRegionalRecognitionSnapshotView(RegionalRecognitionSnapshot item)
    {
        return new RegionalRecognitionSnapshotView(
            item.Code,
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 80),
            Text.Truncate(item.EmployeeId, 64),
            item.JobCode,
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalRecognitionSnapshotView> ToRegionalRecognitionSnapshotViews(List<RegionalRecognitionSnapshot> items)
    {
        var views = new List<RegionalRecognitionSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalRecognitionSnapshotView(item));
        }
        return views;
    }

    public static double SumRegionalRecognitionSnapshotView(RegionalRecognitionSnapshotView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedBudget + parsedAmount;
    }
}
