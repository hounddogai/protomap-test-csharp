namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using System.Globalization;

/// <summary>How screens and exports show a training due proposal.</summary>
public sealed record TrainingDueProposalView(string CodeText, string NameText, string ManagerId, string Limit, string Quantity, string Cost, string Minimum, string Skill, string Priority, string StatusLabel)
{
    public static TrainingDueProposalView ToTrainingDueProposalView(TrainingDueProposal item)
    {
        return new TrainingDueProposalView(
            item.Code,
            item.Name,
            item.ManagerId,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueProposalView> ToTrainingDueProposalViews(List<TrainingDueProposal> items)
    {
        var views = new List<TrainingDueProposalView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueProposalView(item));
        }
        return views;
    }

    public static double SumTrainingDueProposalView(TrainingDueProposalView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedQuantity + parsedCost;
    }
}
