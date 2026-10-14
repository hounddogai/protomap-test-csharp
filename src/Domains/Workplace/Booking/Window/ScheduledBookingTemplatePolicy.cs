namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled booking template policy.</summary>
public static class ScheduledBookingTemplatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledBookingTemplate()
    {
        var policy = Retry.DefaultPolicy("scheduled-booking-template");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeScheduledBookingTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a scheduled booking template code in its canonical SBT- form.</summary>
    public static string NormalizeScheduledBookingTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
