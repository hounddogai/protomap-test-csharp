namespace Acme.Workforce.Payrollops.PayRun.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped pay run checkpoint.</summary>
public sealed record CappedPayRunCheckpointView(string CodeText, string Name, string Revision, string Version, string RegionText, string Title, string Summary, string FteText, string StatusLabel)
{
    public static CappedPayRunCheckpointView ToCappedPayRunCheckpointView(CappedPayRunCheckpoint item)
    {
        return new CappedPayRunCheckpointView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Region,
            Text.Truncate(item.Title, 120),
            item.Summary,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPayRunCheckpointView> ToCappedPayRunCheckpointViews(List<CappedPayRunCheckpoint> items)
    {
        var views = new List<CappedPayRunCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPayRunCheckpointView(item));
        }
        return views;
    }

    public static double SumCappedPayRunCheckpointView(CappedPayRunCheckpointView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedVersion + parsedFte;
    }
}
