namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted training due profile.</summary>
public sealed record WeightedTrainingDueProfileView(string Code, string Title, string Reviewer, string Fte, string Amount, string Sequence, string Skill, string StatusLabel)
{
    public static WeightedTrainingDueProfileView ToWeightedTrainingDueProfileView(WeightedTrainingDueProfile item)
    {
        return new WeightedTrainingDueProfileView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Reviewer,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedTrainingDueProfileView> ToWeightedTrainingDueProfileViews(List<WeightedTrainingDueProfile> items)
    {
        var views = new List<WeightedTrainingDueProfileView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedTrainingDueProfileView(item));
        }
        return views;
    }

    public static double SumWeightedTrainingDueProfileView(WeightedTrainingDueProfileView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedFte + parsedAmount + (double)parsedSequence;
    }
}
