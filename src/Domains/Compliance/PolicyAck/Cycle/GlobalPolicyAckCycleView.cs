namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global policy ack cycle.</summary>
public sealed record GlobalPolicyAckCycleView(string Code, string Title, string Retries, string Badge, string PhoneText, string StartsAt, string Attempts, string StatusLabel)
{
    public static GlobalPolicyAckCycleView ToGlobalPolicyAckCycleView(GlobalPolicyAckCycle item)
    {
        return new GlobalPolicyAckCycleView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            item.Phone,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPolicyAckCycleView> ToGlobalPolicyAckCycleViews(List<GlobalPolicyAckCycle> items)
    {
        var views = new List<GlobalPolicyAckCycleView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPolicyAckCycleView(item));
        }
        return views;
    }

    public static double SumGlobalPolicyAckCycleView(GlobalPolicyAckCycleView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedStartsAt + (double)parsedAttempts;
    }
}
