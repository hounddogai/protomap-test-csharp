namespace Acme.Workforce.Payrollops.Tests.Garnishment.Variance;

using Acme.Workforce.Payrollops.Garnishment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GarnishmentVarianceChecks
{
    public static void GarnishmentVarianceStartsInCreated()
    {
        var item = GarnishmentVariance.NewGarnishmentVariance("code-858", "label-818");
        Check.Equal(item.Status, GarnishmentVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentVarianceMovesToValidated()
    {
        var item = GarnishmentVariance.NewGarnishmentVariance("code-858", "label-818");
        try
        {
            item.MoveTo(GarnishmentVarianceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentVarianceStatus.Validated, "status");
    }

    public static void GarnishmentVarianceRejectsMovesFromRetired()
    {
        var item = GarnishmentVariance.NewGarnishmentVariance("code-858", "label-818");
        item.Status = GarnishmentVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(GarnishmentVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        GarnishmentVarianceStartsInCreated();
        GarnishmentVarianceMovesToValidated();
        GarnishmentVarianceRejectsMovesFromRetired();
    }
}
