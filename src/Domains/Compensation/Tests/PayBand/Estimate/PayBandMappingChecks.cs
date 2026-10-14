namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayBandMappingChecks
{
    public static void PayBandMappingStartsInOpen()
    {
        var item = PayBandMapping.NewPayBandMapping("code-381", "label-10");
        Check.Equal(item.Status, PayBandMappingStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandMappingMovesToReopened()
    {
        var item = PayBandMapping.NewPayBandMapping("code-381", "label-10");
        try
        {
            item.MoveTo(PayBandMappingStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandMappingStatus.Reopened, "status");
    }

    public static void PayBandMappingRejectsMovesFromArchived()
    {
        var item = PayBandMapping.NewPayBandMapping("code-381", "label-10");
        item.Status = PayBandMappingStatus.Archived;
        Check.Fails(() => item.MoveTo(PayBandMappingStatus.Open), "move");
    }

    public static void RunAll()
    {
        PayBandMappingStartsInOpen();
        PayBandMappingMovesToReopened();
        PayBandMappingRejectsMovesFromArchived();
    }
}
