namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SuccessionPlanQuotaChecks
{
    public static void SuccessionPlanQuotaStartsInCreated()
    {
        var item = SuccessionPlanQuota.NewSuccessionPlanQuota("code-924", "title-683");
        Check.Equal(item.Status, SuccessionPlanQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanQuotaMovesToRetired()
    {
        var item = SuccessionPlanQuota.NewSuccessionPlanQuota("code-924", "title-683");
        try
        {
            item.MoveTo(SuccessionPlanQuotaStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanQuotaStatus.Retired, "status");
    }

    public static void SuccessionPlanQuotaRejectsMovesFromRetired()
    {
        var item = SuccessionPlanQuota.NewSuccessionPlanQuota("code-924", "title-683");
        item.Status = SuccessionPlanQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(SuccessionPlanQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanQuotaStartsInCreated();
        SuccessionPlanQuotaMovesToRetired();
        SuccessionPlanQuotaRejectsMovesFromRetired();
    }
}
