namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LegacyActionPlanDigestChecks
{
    public static void LegacyActionPlanDigestStartsInCreated()
    {
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("code-84", "title-526");
        Check.Equal(item.Status, LegacyActionPlanDigestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyActionPlanDigestMovesToPublished()
    {
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("code-84", "title-526");
        try
        {
            item.MoveTo(LegacyActionPlanDigestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyActionPlanDigestStatus.Published, "status");
    }

    public static void LegacyActionPlanDigestRejectsMovesFromRetired()
    {
        var item = LegacyActionPlanDigest.NewLegacyActionPlanDigest("code-84", "title-526");
        item.Status = LegacyActionPlanDigestStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyActionPlanDigestStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyActionPlanDigestStartsInCreated();
        LegacyActionPlanDigestMovesToPublished();
        LegacyActionPlanDigestRejectsMovesFromRetired();
    }
}
