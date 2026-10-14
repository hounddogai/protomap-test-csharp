namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayProfileChecks
{
    public static void RetroPayProfileStartsInCreated()
    {
        var item = RetroPayProfile.NewRetroPayProfile("code-334", "title-488");
        Check.Equal(item.Status, RetroPayProfileStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayProfileMovesToValidated()
    {
        var item = RetroPayProfile.NewRetroPayProfile("code-334", "title-488");
        try
        {
            item.MoveTo(RetroPayProfileStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayProfileStatus.Validated, "status");
    }

    public static void RetroPayProfileRejectsMovesFromRetired()
    {
        var item = RetroPayProfile.NewRetroPayProfile("code-334", "title-488");
        item.Status = RetroPayProfileStatus.Retired;
        Check.Fails(() => item.MoveTo(RetroPayProfileStatus.Created), "move");
    }

    public static void RunAll()
    {
        RetroPayProfileStartsInCreated();
        RetroPayProfileMovesToValidated();
        RetroPayProfileRejectsMovesFromRetired();
    }
}
