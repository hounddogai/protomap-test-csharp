namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionTargetChecks
{
    public static void LegacyPositionTargetStartsInOpen()
    {
        var item = LegacyPositionTarget.NewLegacyPositionTarget("code-666", "title-561");
        Check.Equal(item.Status, LegacyPositionTargetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPositionTargetMovesToReopened()
    {
        var item = LegacyPositionTarget.NewLegacyPositionTarget("code-666", "title-561");
        try
        {
            item.MoveTo(LegacyPositionTargetStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPositionTargetStatus.Reopened, "status");
    }

    public static void LegacyPositionTargetRejectsMovesFromArchived()
    {
        var item = LegacyPositionTarget.NewLegacyPositionTarget("code-666", "title-561");
        item.Status = LegacyPositionTargetStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyPositionTargetStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyPositionTargetStartsInOpen();
        LegacyPositionTargetMovesToReopened();
        LegacyPositionTargetRejectsMovesFromArchived();
    }
}
