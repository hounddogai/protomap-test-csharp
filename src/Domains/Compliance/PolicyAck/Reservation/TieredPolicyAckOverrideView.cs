namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered policy ack override.</summary>
public sealed record TieredPolicyAckOverrideView(string Code, string Label, string Hours, string DurationMinutesText, string CostCenter, string Attempts, string PhoneText, string Source, string StatusLabel)
{
    public static TieredPolicyAckOverrideView ToTieredPolicyAckOverrideView(TieredPolicyAckOverride item)
    {
        return new TieredPolicyAckOverrideView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPolicyAckOverrideView> ToTieredPolicyAckOverrideViews(List<TieredPolicyAckOverride> items)
    {
        var views = new List<TieredPolicyAckOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPolicyAckOverrideView(item));
        }
        return views;
    }

    public static double SumTieredPolicyAckOverrideView(TieredPolicyAckOverrideView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedDurationMinutes + (double)parsedAttempts;
    }
}
