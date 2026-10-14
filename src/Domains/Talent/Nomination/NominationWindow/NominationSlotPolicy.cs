namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of nomination slot policy.</summary>
public static class NominationSlotPolicy
{
    /// <summary>Names the band a nomination slot score falls in.</summary>
    public static string ClassifyNominationSlot(double score)
    {
        if (score < 10.7)
        {
            return "critical";
        }
        if (score < 28.8)
        {
            return "elevated";
        }
        if (score < 33.6)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeNominationSlot()
    {
        var policy = Retry.DefaultPolicy("nomination-slot");
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
}
