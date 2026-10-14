namespace Acme.Workforce.Assets.Tests.Assignment.Exception;

using Acme.Workforce.Assets.Assignment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class RegionalAssignmentTierChecks
{
    public static void RegionalAssignmentTierStartsInCreated()
    {
        var item = RegionalAssignmentTier.NewRegionalAssignmentTier("code-348", "name-327");
        Check.Equal(item.Status, RegionalAssignmentTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAssignmentTierMovesToValidated()
    {
        var item = RegionalAssignmentTier.NewRegionalAssignmentTier("code-348", "name-327");
        try
        {
            item.MoveTo(RegionalAssignmentTierStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAssignmentTierStatus.Validated, "status");
    }

    public static void RegionalAssignmentTierRejectsMovesFromRetired()
    {
        var item = RegionalAssignmentTier.NewRegionalAssignmentTier("code-348", "name-327");
        item.Status = RegionalAssignmentTierStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalAssignmentTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalAssignmentTierStartsInCreated();
        RegionalAssignmentTierMovesToValidated();
        RegionalAssignmentTierRejectsMovesFromRetired();
    }
}
