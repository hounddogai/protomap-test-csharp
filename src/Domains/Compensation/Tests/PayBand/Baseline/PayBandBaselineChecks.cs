namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandBaselineChecks
{
    public static void PayBandBaselineStartsInCreated()
    {
        var item = PayBandBaseline.NewPayBandBaseline("code-342", "title-257");
        Check.Equal(item.Status, PayBandBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandBaselineMovesToPublished()
    {
        var item = PayBandBaseline.NewPayBandBaseline("code-342", "title-257");
        try
        {
            item.MoveTo(PayBandBaselineStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandBaselineStatus.Published, "status");
    }

    public static void PayBandBaselineRejectsMovesFromRetired()
    {
        var item = PayBandBaseline.NewPayBandBaseline("code-342", "title-257");
        item.Status = PayBandBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(PayBandBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayBandBaselineStartsInCreated();
        PayBandBaselineMovesToPublished();
        PayBandBaselineRejectsMovesFromRetired();
    }
}
