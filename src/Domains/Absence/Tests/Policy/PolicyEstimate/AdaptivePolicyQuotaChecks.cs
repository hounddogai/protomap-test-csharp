namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class AdaptivePolicyQuotaChecks
{
    public static void AdaptivePolicyQuotaStartsInRequested()
    {
        var item = AdaptivePolicyQuota.NewAdaptivePolicyQuota("code-330", "title-595");
        Check.Equal(item.Status, AdaptivePolicyQuotaStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePolicyQuotaMovesToGranted()
    {
        var item = AdaptivePolicyQuota.NewAdaptivePolicyQuota("code-330", "title-595");
        try
        {
            item.MoveTo(AdaptivePolicyQuotaStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePolicyQuotaStatus.Granted, "status");
    }

    public static void AdaptivePolicyQuotaRejectsMovesFromExpired()
    {
        var item = AdaptivePolicyQuota.NewAdaptivePolicyQuota("code-330", "title-595");
        item.Status = AdaptivePolicyQuotaStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptivePolicyQuotaStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptivePolicyQuotaStartsInRequested();
        AdaptivePolicyQuotaMovesToGranted();
        AdaptivePolicyQuotaRejectsMovesFromExpired();
    }
}
