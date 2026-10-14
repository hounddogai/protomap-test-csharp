namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AccrualOverrideChecks
{
    public static void AccrualOverrideStartsInOpen()
    {
        var item = AccrualOverride.NewAccrualOverride("code-645", "title-339");
        Check.Equal(item.Status, AccrualOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualOverrideMovesToResolved()
    {
        var item = AccrualOverride.NewAccrualOverride("code-645", "title-339");
        try
        {
            item.MoveTo(AccrualOverrideStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualOverrideStatus.Resolved, "status");
    }

    public static void AccrualOverrideRejectsMovesFromArchived()
    {
        var item = AccrualOverride.NewAccrualOverride("code-645", "title-339");
        item.Status = AccrualOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(AccrualOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccrualOverrideStartsInOpen();
        AccrualOverrideMovesToResolved();
        AccrualOverrideRejectsMovesFromArchived();
    }
}
