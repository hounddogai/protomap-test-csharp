namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class LenientPositionProfileChecks
{
    public static void LenientPositionProfileStartsInProposed()
    {
        var item = LenientPositionProfile.NewLenientPositionProfile("code-476", "title-553");
        Check.Equal(item.Status, LenientPositionProfileStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPositionProfileMovesToApproved()
    {
        var item = LenientPositionProfile.NewLenientPositionProfile("code-476", "title-553");
        try
        {
            item.MoveTo(LenientPositionProfileStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPositionProfileStatus.Approved, "status");
    }

    public static void LenientPositionProfileRejectsMovesFromWithdrawn()
    {
        var item = LenientPositionProfile.NewLenientPositionProfile("code-476", "title-553");
        item.Status = LenientPositionProfileStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientPositionProfileStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientPositionProfileStartsInProposed();
        LenientPositionProfileMovesToApproved();
        LenientPositionProfileRejectsMovesFromWithdrawn();
    }
}
