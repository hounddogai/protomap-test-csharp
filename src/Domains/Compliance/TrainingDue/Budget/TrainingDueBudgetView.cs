namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due budget.</summary>
public sealed record TrainingDueBudgetView(string Code, string Name, string EndsAt, string ExternalRef, string Fte, string BadgeText, string Locale, string JobCode, string Timezone, string Segment, string StatusLabel)
{
    public static TrainingDueBudgetView ToTrainingDueBudgetView(TrainingDueBudget item)
    {
        return new TrainingDueBudgetView(
            item.Code,
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Locale,
            item.JobCode,
            item.Timezone,
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueBudgetView> ToTrainingDueBudgetViews(List<TrainingDueBudget> items)
    {
        var views = new List<TrainingDueBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueBudgetView(item));
        }
        return views;
    }

    public static double SumTrainingDueBudgetView(TrainingDueBudgetView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedFte;
    }
}
