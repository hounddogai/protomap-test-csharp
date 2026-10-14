namespace Acme.Workforce.Contractors.Rate.Segment;

using System.Globalization;

/// <summary>How screens and exports show a rate assignment.</summary>
public sealed record RateAssignmentView(string Code, string Title, string BatchSize, string Checksum, string Amount, string BudgetText, string MaximumText, string Rate, string SkillText, string ThresholdText, string StatusLabel)
{
    public static RateAssignmentView ToRateAssignmentView(RateAssignment item)
    {
        return new RateAssignmentView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateAssignmentView> ToRateAssignmentViews(List<RateAssignment> items)
    {
        var views = new List<RateAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToRateAssignmentView(item));
        }
        return views;
    }

    public static double SumRateAssignmentView(RateAssignmentView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedAmount + parsedBudget;
    }
}
