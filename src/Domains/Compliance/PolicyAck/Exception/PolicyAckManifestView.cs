namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack manifest.</summary>
public sealed record PolicyAckManifestView(string Code, string LabelText, string WindowDaysText, string Locale, string Title, string TenureMonths, string Approved, string Currency, string StatusLabel)
{
    public static PolicyAckManifestView ToPolicyAckManifestView(PolicyAckManifest item)
    {
        return new PolicyAckManifestView(
            item.Code,
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 64),
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            Text.Truncate(item.Currency, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckManifestView> ToPolicyAckManifestViews(List<PolicyAckManifest> items)
    {
        var views = new List<PolicyAckManifestView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckManifestView(item));
        }
        return views;
    }

    public static double SumPolicyAckManifestView(PolicyAckManifestView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedTenureMonths;
    }
}
