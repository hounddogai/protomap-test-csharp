namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual exception manifest.</summary>
public sealed record ManualExceptionManifestView(string Code, string Name, string Pinned, string ChannelText, string CostCenterText, string Revision, string UpdatedAtText, string StatusLabel)
{
    public static ManualExceptionManifestView ToManualExceptionManifestView(ManualExceptionManifest item)
    {
        return new ManualExceptionManifestView(
            item.Code,
            Text.Truncate(item.Name, 64),
            $"{item.Pinned}",
            Text.Truncate(item.Channel, 40),
            Text.Truncate(item.CostCenter, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualExceptionManifestView> ToManualExceptionManifestViews(List<ManualExceptionManifest> items)
    {
        var views = new List<ManualExceptionManifestView>();
        foreach (var item in items)
        {
            views.Add(ToManualExceptionManifestView(item));
        }
        return views;
    }

    public static double SumManualExceptionManifestView(ManualExceptionManifestView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedUpdatedAt;
    }
}
