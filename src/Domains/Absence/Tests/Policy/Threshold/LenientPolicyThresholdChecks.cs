namespace Acme.Workforce.Absence.Tests.Policy.Threshold;

using Acme.Workforce.Absence.Policy.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientPolicyThresholdChecks
{
    public static void LenientPolicyThresholdStartsInIdle()
    {
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("code-693", "name-137");
        Check.Equal(item.Status, LenientPolicyThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPolicyThresholdMovesToDraining()
    {
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("code-693", "name-137");
        try
        {
            item.MoveTo(LenientPolicyThresholdStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPolicyThresholdStatus.Draining, "status");
    }

    public static void LenientPolicyThresholdRejectsMovesFromStopped()
    {
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("code-693", "name-137");
        item.Status = LenientPolicyThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientPolicyThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientPolicyThresholdStartsInIdle();
        LenientPolicyThresholdMovesToDraining();
        LenientPolicyThresholdRejectsMovesFromStopped();
    }
}
