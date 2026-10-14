namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped statement of work assignment.</summary>
public sealed record CappedStatementOfWorkAssignmentView(string CodeText, string Name, string Revision, string Ceiling, string Version, string Cost, string LabelText, string TeamText, string StatusLabel)
{
    public static CappedStatementOfWorkAssignmentView ToCappedStatementOfWorkAssignmentView(CappedStatementOfWorkAssignment item)
    {
        return new CappedStatementOfWorkAssignmentView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedStatementOfWorkAssignmentView> ToCappedStatementOfWorkAssignmentViews(List<CappedStatementOfWorkAssignment> items)
    {
        var views = new List<CappedStatementOfWorkAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToCappedStatementOfWorkAssignmentView(item));
        }
        return views;
    }

    public static double SumCappedStatementOfWorkAssignmentView(CappedStatementOfWorkAssignmentView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedCeiling + (double)parsedVersion;
    }
}
