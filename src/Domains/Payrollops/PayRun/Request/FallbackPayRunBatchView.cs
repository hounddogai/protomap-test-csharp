namespace Acme.Workforce.Payrollops.PayRun.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback pay run batch.</summary>
public sealed record FallbackPayRunBatchView(string Code, string NameText, string OwnerId, string VersionText, string SourceText, string UnitText, string Revision, string ContactEmail, string Label, string ManagerIdText, string StatusLabel)
{
    public static FallbackPayRunBatchView ToFallbackPayRunBatchView(FallbackPayRunBatch item)
    {
        return new FallbackPayRunBatchView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.OwnerId,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Unit,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Label,
            Text.Truncate(item.ManagerId, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPayRunBatchView> ToFallbackPayRunBatchViews(List<FallbackPayRunBatch> items)
    {
        var views = new List<FallbackPayRunBatchView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPayRunBatchView(item));
        }
        return views;
    }

    public static double SumFallbackPayRunBatchView(FallbackPayRunBatchView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedRevision;
    }
}
