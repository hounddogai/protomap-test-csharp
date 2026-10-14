namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due review.</summary>
public sealed record TrainingDueReviewView(string CodeText, string Label, string Limit, string BudgetText, string LeaveDays, string Active, string Unit, string Title, string Score, string ExpiresAt, string Channel, string StatusLabel)
{
    public static TrainingDueReviewView ToTrainingDueReviewView(TrainingDueReview item)
    {
        return new TrainingDueReviewView(
            item.Code,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Unit,
            Text.Truncate(item.Title, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueReviewView> ToTrainingDueReviewViews(List<TrainingDueReview> items)
    {
        var views = new List<TrainingDueReviewView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueReviewView(item));
        }
        return views;
    }

    public static double SumTrainingDueReviewView(TrainingDueReviewView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedBudget + (double)parsedLeaveDays;
    }
}
