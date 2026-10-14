namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueClaimChecks
{
    public static void AdaptiveTrainingDueClaimStartsInCreated()
    {
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("code-2", "title-354");
        Check.Equal(item.Status, AdaptiveTrainingDueClaimStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTrainingDueClaimMovesToValidated()
    {
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("code-2", "title-354");
        try
        {
            item.MoveTo(AdaptiveTrainingDueClaimStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTrainingDueClaimStatus.Validated, "status");
    }

    public static void AdaptiveTrainingDueClaimRejectsMovesFromRetired()
    {
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("code-2", "title-354");
        item.Status = AdaptiveTrainingDueClaimStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveTrainingDueClaimStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDueClaimStartsInCreated();
        AdaptiveTrainingDueClaimMovesToValidated();
        AdaptiveTrainingDueClaimRejectsMovesFromRetired();
    }
}
