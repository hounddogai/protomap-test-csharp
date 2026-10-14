namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class EngagementProfileChecks
{
    public static void EngagementProfileStartsInRequested()
    {
        var item = EngagementProfile.NewEngagementProfile("code-277", "name-353");
        Check.Equal(item.Status, EngagementProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementProfileMovesToRevoked()
    {
        var item = EngagementProfile.NewEngagementProfile("code-277", "name-353");
        try
        {
            item.MoveTo(EngagementProfileStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementProfileStatus.Revoked, "status");
    }

    public static void EngagementProfileRejectsMovesFromExpired()
    {
        var item = EngagementProfile.NewEngagementProfile("code-277", "name-353");
        item.Status = EngagementProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(EngagementProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        EngagementProfileStartsInRequested();
        EngagementProfileMovesToRevoked();
        EngagementProfileRejectsMovesFromExpired();
    }
}
