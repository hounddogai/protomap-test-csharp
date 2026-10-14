namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due milestone.</summary>
public sealed record TrainingDueMilestoneView(string Code, string LabelText, string OwnerId, string Description, string NameText, string JobCode, string Channel, string ScoreText, string StatusLabel)
{
    public static TrainingDueMilestoneView ToTrainingDueMilestoneView(TrainingDueMilestone item)
    {
        return new TrainingDueMilestoneView(
            item.Code,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.OwnerId, 120),
            item.Description,
            item.Name,
            Text.Truncate(item.JobCode, 40),
            Text.Truncate(item.Channel, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueMilestoneView> ToTrainingDueMilestoneViews(List<TrainingDueMilestone> items)
    {
        var views = new List<TrainingDueMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueMilestoneView(item));
        }
        return views;
    }

    public static double SumTrainingDueMilestoneView(TrainingDueMilestoneView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
