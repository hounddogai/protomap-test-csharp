namespace Acme.Workforce.Workplace.Tests.Desk.DeskBudget;

using Acme.Workforce.Workplace.Desk.DeskBudget;

/// <summary>Checks of desk budget code, run by the project's runner.</summary>
public static class RollingDeskMappingChecks
{
    public static void RollingDeskMappingStartsInRequested()
    {
        var item = RollingDeskMapping.NewRollingDeskMapping("code-377", "label-746");
        Check.Equal(item.Status, RollingDeskMappingStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingDeskMappingMovesToRevoked()
    {
        var item = RollingDeskMapping.NewRollingDeskMapping("code-377", "label-746");
        try
        {
            item.MoveTo(RollingDeskMappingStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingDeskMappingStatus.Revoked, "status");
    }

    public static void RollingDeskMappingRejectsMovesFromExpired()
    {
        var item = RollingDeskMapping.NewRollingDeskMapping("code-377", "label-746");
        item.Status = RollingDeskMappingStatus.Expired;
        Check.Fails(() => item.MoveTo(RollingDeskMappingStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RollingDeskMappingStartsInRequested();
        RollingDeskMappingMovesToRevoked();
        RollingDeskMappingRejectsMovesFromExpired();
    }
}
