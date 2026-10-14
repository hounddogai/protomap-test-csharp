namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work metric.</summary>
public sealed record StatementOfWorkMetricView(string Code, string Title, string Team, string Segment, string VersionText, string Minimum, string CostCenterText, string Source, string CreatedAtText, string StatusLabel)
{
    public static StatementOfWorkMetricView ToStatementOfWorkMetricView(StatementOfWorkMetric item)
    {
        return new StatementOfWorkMetricView(
            item.Code,
            item.Title,
            Text.Truncate(item.Team, 40),
            item.Segment,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Source,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkMetricView> ToStatementOfWorkMetricViews(List<StatementOfWorkMetric> items)
    {
        var views = new List<StatementOfWorkMetricView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkMetricView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkMetricView(StatementOfWorkMetricView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedMinimum + (double)parsedCreatedAt;
    }
}
