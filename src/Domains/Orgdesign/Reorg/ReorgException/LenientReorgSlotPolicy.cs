namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient reorg slot policy.</summary>
public static class LenientReorgSlotPolicy
{
    public static double LenientReorgSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.22;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientReorgSlot()
    {
        var policy = Retry.DefaultPolicy("lenient-reorg-slot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a lenient reorg slot score falls in.</summary>
    public static string ClassifyLenientReorgSlot(double score)
    {
        if (score < 21.8)
        {
            return "critical";
        }
        if (score < 82.4)
        {
            return "high";
        }
        if (score < 92.6)
        {
            return "low";
        }
        if (score < 96.6)
        {
            return "watch";
        }
        return "ok";
    }

    public static List<string> MergeLenientReorgSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
