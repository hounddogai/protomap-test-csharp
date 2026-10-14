namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched training due notice.</summary>
public sealed record BatchedTrainingDueNoticeView(string Code, string Name, string SalaryBandText, string Budget, string Category, string Summary, string StatusLabel)
{
    public static BatchedTrainingDueNoticeView ToBatchedTrainingDueNoticeView(BatchedTrainingDueNotice item)
    {
        return new BatchedTrainingDueNoticeView(
            item.Code,
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.SalaryBand, 120),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 80),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTrainingDueNoticeView> ToBatchedTrainingDueNoticeViews(List<BatchedTrainingDueNotice> items)
    {
        var views = new List<BatchedTrainingDueNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTrainingDueNoticeView(item));
        }
        return views;
    }

    public static double SumBatchedTrainingDueNoticeView(BatchedTrainingDueNoticeView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
