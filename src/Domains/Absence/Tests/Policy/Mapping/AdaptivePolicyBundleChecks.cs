namespace Acme.Workforce.Absence.Tests.Policy.Mapping;

using Acme.Workforce.Absence.Policy.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptivePolicyBundleChecks
{
    public static void AdaptivePolicyBundleStartsInPlanned()
    {
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("code-738", "label-214");
        Check.Equal(item.Status, AdaptivePolicyBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePolicyBundleMovesToScheduled()
    {
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("code-738", "label-214");
        try
        {
            item.MoveTo(AdaptivePolicyBundleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePolicyBundleStatus.Scheduled, "status");
    }

    public static void AdaptivePolicyBundleRejectsMovesFromCancelled()
    {
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("code-738", "label-214");
        item.Status = AdaptivePolicyBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptivePolicyBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptivePolicyBundleStartsInPlanned();
        AdaptivePolicyBundleMovesToScheduled();
        AdaptivePolicyBundleRejectsMovesFromCancelled();
    }
}
