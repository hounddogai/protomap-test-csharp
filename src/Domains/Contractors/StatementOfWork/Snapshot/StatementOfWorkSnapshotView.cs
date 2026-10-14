namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work snapshot.</summary>
public sealed record StatementOfWorkSnapshotView(string Code, string Label, string Team, string Ratio, string Cost, string Revision, string Hours, string FloorValueText, string StatusLabel)
{
    public static StatementOfWorkSnapshotView ToStatementOfWorkSnapshotView(StatementOfWorkSnapshot item)
    {
        return new StatementOfWorkSnapshotView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Team,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkSnapshotView> ToStatementOfWorkSnapshotViews(List<StatementOfWorkSnapshot> items)
    {
        var views = new List<StatementOfWorkSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkSnapshotView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkSnapshotView(StatementOfWorkSnapshotView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCost + (double)parsedRevision;
    }
}
