namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped cost model assignment.</summary>
public sealed record CappedCostModelAssignmentView(string Code, string Name, string ChannelText, string BudgetText, string MaximumText, string StatusLabel)
{
    public static CappedCostModelAssignmentView ToCappedCostModelAssignmentView(CappedCostModelAssignment item)
    {
        return new CappedCostModelAssignmentView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Channel,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedCostModelAssignmentView> ToCappedCostModelAssignmentViews(List<CappedCostModelAssignment> items)
    {
        var views = new List<CappedCostModelAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToCappedCostModelAssignmentView(item));
        }
        return views;
    }

    public static double SumCappedCostModelAssignmentView(CappedCostModelAssignmentView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMaximum;
    }
}
