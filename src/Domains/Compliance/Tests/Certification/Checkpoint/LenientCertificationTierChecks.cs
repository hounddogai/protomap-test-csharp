namespace Acme.Workforce.Compliance.Tests.Certification.Checkpoint;

using Acme.Workforce.Compliance.Certification.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LenientCertificationTierChecks
{
    public static void LenientCertificationTierStartsInPlanned()
    {
        var item = LenientCertificationTier.NewLenientCertificationTier("code-459", "name-677");
        Check.Equal(item.Status, LenientCertificationTierStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientCertificationTierMovesToCompleted()
    {
        var item = LenientCertificationTier.NewLenientCertificationTier("code-459", "name-677");
        try
        {
            item.MoveTo(LenientCertificationTierStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientCertificationTierStatus.Completed, "status");
    }

    public static void LenientCertificationTierRejectsMovesFromCancelled()
    {
        var item = LenientCertificationTier.NewLenientCertificationTier("code-459", "name-677");
        item.Status = LenientCertificationTierStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientCertificationTierStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientCertificationTierStartsInPlanned();
        LenientCertificationTierMovesToCompleted();
        LenientCertificationTierRejectsMovesFromCancelled();
    }
}
