namespace Acme.Workforce.Talent.Tests.CareerPath.Quota;

using Acme.Workforce.Talent.CareerPath.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PendingCareerPathQuotaChecks
{
    public static void PendingCareerPathQuotaStartsInProposed()
    {
        var item = PendingCareerPathQuota.NewPendingCareerPathQuota("code-442", "title-958");
        Check.Equal(item.Status, PendingCareerPathQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingCareerPathQuotaMovesToRejected()
    {
        var item = PendingCareerPathQuota.NewPendingCareerPathQuota("code-442", "title-958");
        try
        {
            item.MoveTo(PendingCareerPathQuotaStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingCareerPathQuotaStatus.Rejected, "status");
    }

    public static void PendingCareerPathQuotaRejectsMovesFromWithdrawn()
    {
        var item = PendingCareerPathQuota.NewPendingCareerPathQuota("code-442", "title-958");
        item.Status = PendingCareerPathQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingCareerPathQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingCareerPathQuotaStartsInProposed();
        PendingCareerPathQuotaMovesToRejected();
        PendingCareerPathQuotaRejectsMovesFromWithdrawn();
    }
}
