namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PrimaryReorgBaselineChecks
{
    public static void PrimaryReorgBaselineStartsInOpen()
    {
        var item = PrimaryReorgBaseline.NewPrimaryReorgBaseline("code-363", "label-999");
        Check.Equal(item.Status, PrimaryReorgBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryReorgBaselineMovesToResolved()
    {
        var item = PrimaryReorgBaseline.NewPrimaryReorgBaseline("code-363", "label-999");
        try
        {
            item.MoveTo(PrimaryReorgBaselineStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryReorgBaselineStatus.Resolved, "status");
    }

    public static void PrimaryReorgBaselineRejectsMovesFromArchived()
    {
        var item = PrimaryReorgBaseline.NewPrimaryReorgBaseline("code-363", "label-999");
        item.Status = PrimaryReorgBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryReorgBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryReorgBaselineStartsInOpen();
        PrimaryReorgBaselineMovesToResolved();
        PrimaryReorgBaselineRejectsMovesFromArchived();
    }
}
