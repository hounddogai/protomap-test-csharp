namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using System.Globalization;

/// <summary>How screens and exports show a adaptive statement of work assignment.</summary>
public sealed record AdaptiveStatementOfWorkAssignmentView(string Code, string Title, string RatioText, string Skill, string Source, string Approved, string StatusLabel)
{
    public static AdaptiveStatementOfWorkAssignmentView ToAdaptiveStatementOfWorkAssignmentView(AdaptiveStatementOfWorkAssignment item)
    {
        return new AdaptiveStatementOfWorkAssignmentView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Source,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveStatementOfWorkAssignmentView> ToAdaptiveStatementOfWorkAssignmentViews(List<AdaptiveStatementOfWorkAssignment> items)
    {
        var views = new List<AdaptiveStatementOfWorkAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveStatementOfWorkAssignmentView(item));
        }
        return views;
    }

    public static double SumAdaptiveStatementOfWorkAssignmentView(AdaptiveStatementOfWorkAssignmentView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
