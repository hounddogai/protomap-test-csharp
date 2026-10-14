namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using System.Globalization;

/// <summary>How screens and exports show a fallback policy ack profile.</summary>
public sealed record FallbackPolicyAckProfileView(string Code, string NameText, string Locale, string Team, string GraceDays, string VersionText, string Description, string Visible, string StatusLabel)
{
    public static FallbackPolicyAckProfileView ToFallbackPolicyAckProfileView(FallbackPolicyAckProfile item)
    {
        return new FallbackPolicyAckProfileView(
            item.Code,
            item.Name,
            item.Locale,
            item.Team,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPolicyAckProfileView> ToFallbackPolicyAckProfileViews(List<FallbackPolicyAckProfile> items)
    {
        var views = new List<FallbackPolicyAckProfileView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPolicyAckProfileView(item));
        }
        return views;
    }

    public static double SumFallbackPolicyAckProfileView(FallbackPolicyAckProfileView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedVersion;
    }
}
