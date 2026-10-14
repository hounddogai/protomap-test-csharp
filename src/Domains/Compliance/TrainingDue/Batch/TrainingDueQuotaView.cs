namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due quota.</summary>
public sealed record TrainingDueQuotaView(string Code, string LabelText, string Description, string MinimumText, string BudgetText, string TenureMonths, string Skill, string Channel, string CreatedAt, string StatusLabel)
{
    public static TrainingDueQuotaView ToTrainingDueQuotaView(TrainingDueQuota item)
    {
        return new TrainingDueQuotaView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Description,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Channel,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueQuotaView> ToTrainingDueQuotaViews(List<TrainingDueQuota> items)
    {
        var views = new List<TrainingDueQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueQuotaView(item));
        }
        return views;
    }

    public static double SumTrainingDueQuotaView(TrainingDueQuotaView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedBudget + (double)parsedTenureMonths;
    }
}
