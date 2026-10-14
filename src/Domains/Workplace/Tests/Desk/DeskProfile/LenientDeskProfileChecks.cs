namespace Acme.Workforce.Workplace.Tests.Desk.DeskProfile;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Checks of desk profile code, run by the project's runner.</summary>
public static class LenientDeskProfileChecks
{
    public static void LenientDeskProfileStartsInCreated()
    {
        var item = LenientDeskProfile.NewLenientDeskProfile("code-343", "name-777");
        Check.Equal(item.Status, LenientDeskProfileStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientDeskProfileMovesToValidated()
    {
        var item = LenientDeskProfile.NewLenientDeskProfile("code-343", "name-777");
        try
        {
            item.MoveTo(LenientDeskProfileStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientDeskProfileStatus.Validated, "status");
    }

    public static void LenientDeskProfileRejectsMovesFromRetired()
    {
        var item = LenientDeskProfile.NewLenientDeskProfile("code-343", "name-777");
        item.Status = LenientDeskProfileStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientDeskProfileStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientDeskProfileStartsInCreated();
        LenientDeskProfileMovesToValidated();
        LenientDeskProfileRejectsMovesFromRetired();
    }
}
