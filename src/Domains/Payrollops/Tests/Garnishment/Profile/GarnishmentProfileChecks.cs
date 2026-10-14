namespace Acme.Workforce.Payrollops.Tests.Garnishment.Profile;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GarnishmentProfileChecks
{
    public static void GarnishmentProfileStartsInCreated()
    {
        var item = GarnishmentProfile.NewGarnishmentProfile("code-423", "title-546");
        Check.Equal(item.Status, GarnishmentProfileStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentProfileMovesToPublished()
    {
        var item = GarnishmentProfile.NewGarnishmentProfile("code-423", "title-546");
        try
        {
            item.MoveTo(GarnishmentProfileStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentProfileStatus.Published, "status");
    }

    public static void GarnishmentProfileRejectsMovesFromRetired()
    {
        var item = GarnishmentProfile.NewGarnishmentProfile("code-423", "title-546");
        item.Status = GarnishmentProfileStatus.Retired;
        Check.Fails(() => item.MoveTo(GarnishmentProfileStatus.Created), "move");
    }

    public static void RunAll()
    {
        GarnishmentProfileStartsInCreated();
        GarnishmentProfileMovesToPublished();
        GarnishmentProfileRejectsMovesFromRetired();
    }
}
