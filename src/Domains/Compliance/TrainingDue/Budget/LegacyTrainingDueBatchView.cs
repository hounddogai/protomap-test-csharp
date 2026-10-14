namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy training due batch.</summary>
public sealed record LegacyTrainingDueBatchView(string Code, string NameText, string SalaryBand, string Region, string Retries, string Phone, string StatusLabel)
{
    public static LegacyTrainingDueBatchView ToLegacyTrainingDueBatchView(LegacyTrainingDueBatch item)
    {
        return new LegacyTrainingDueBatchView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 40),
            item.SalaryBand,
            item.Region,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTrainingDueBatchView> ToLegacyTrainingDueBatchViews(List<LegacyTrainingDueBatch> items)
    {
        var views = new List<LegacyTrainingDueBatchView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTrainingDueBatchView(item));
        }
        return views;
    }

    public static double SumLegacyTrainingDueBatchView(LegacyTrainingDueBatchView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
