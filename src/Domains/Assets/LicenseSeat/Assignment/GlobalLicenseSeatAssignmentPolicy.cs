namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global license seat assignment policy.</summary>
public static class GlobalLicenseSeatAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalLicenseSeatAssignment()
    {
        var policy = Retry.DefaultPolicy("global-license-seat-assignment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string GlobalLicenseSeatAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double GlobalLicenseSeatAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeGlobalLicenseSeatAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
