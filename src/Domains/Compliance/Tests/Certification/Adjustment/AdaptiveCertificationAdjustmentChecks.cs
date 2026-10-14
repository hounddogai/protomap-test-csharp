namespace Acme.Workforce.Compliance.Tests.Certification.Adjustment;

using Acme.Workforce.Compliance.Certification.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AdaptiveCertificationAdjustmentChecks
{
    public static void AdaptiveCertificationAdjustmentStartsInCreated()
    {
        var item = AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment("code-79", "name-158");
        Check.Equal(item.Status, AdaptiveCertificationAdjustmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveCertificationAdjustmentMovesToValidated()
    {
        var item = AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment("code-79", "name-158");
        try
        {
            item.MoveTo(AdaptiveCertificationAdjustmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveCertificationAdjustmentStatus.Validated, "status");
    }

    public static void AdaptiveCertificationAdjustmentRejectsMovesFromRetired()
    {
        var item = AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment("code-79", "name-158");
        item.Status = AdaptiveCertificationAdjustmentStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveCertificationAdjustmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveCertificationAdjustmentStartsInCreated();
        AdaptiveCertificationAdjustmentMovesToValidated();
        AdaptiveCertificationAdjustmentRejectsMovesFromRetired();
    }
}
