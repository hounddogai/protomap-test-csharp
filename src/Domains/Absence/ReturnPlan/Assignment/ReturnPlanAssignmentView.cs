namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a return plan assignment.</summary>
public sealed record ReturnPlanAssignmentView(string Code, string NameText, string Fte, string SalaryBand, string Summary, string StatusLabel)
{
    public static ReturnPlanAssignmentView ToReturnPlanAssignmentView(ReturnPlanAssignment item)
    {
        return new ReturnPlanAssignmentView(
            item.Code,
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanAssignmentView> ToReturnPlanAssignmentViews(List<ReturnPlanAssignment> items)
    {
        var views = new List<ReturnPlanAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanAssignmentView(item));
        }
        return views;
    }

    public static double SumReturnPlanAssignmentView(ReturnPlanAssignmentView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
