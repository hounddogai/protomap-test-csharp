namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatProfileChecks
{
    public static void LicenseSeatProfileStartsInOpen()
    {
        var item = LicenseSeatProfile.NewLicenseSeatProfile("code-979", "title-747");
        Check.Equal(item.Status, LicenseSeatProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatProfileMovesToResolved()
    {
        var item = LicenseSeatProfile.NewLicenseSeatProfile("code-979", "title-747");
        try
        {
            item.MoveTo(LicenseSeatProfileStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatProfileStatus.Resolved, "status");
    }

    public static void LicenseSeatProfileRejectsMovesFromArchived()
    {
        var item = LicenseSeatProfile.NewLicenseSeatProfile("code-979", "title-747");
        item.Status = LicenseSeatProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(LicenseSeatProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        LicenseSeatProfileStartsInOpen();
        LicenseSeatProfileMovesToResolved();
        LicenseSeatProfileRejectsMovesFromArchived();
    }
}
