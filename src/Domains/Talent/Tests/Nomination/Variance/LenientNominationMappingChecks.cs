namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LenientNominationMappingChecks
{
    public static void LenientNominationMappingStartsInCreated()
    {
        var item = LenientNominationMapping.NewLenientNominationMapping("code-362", "name-109");
        Check.Equal(item.Status, LenientNominationMappingStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientNominationMappingMovesToValidated()
    {
        var item = LenientNominationMapping.NewLenientNominationMapping("code-362", "name-109");
        try
        {
            item.MoveTo(LenientNominationMappingStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientNominationMappingStatus.Validated, "status");
    }

    public static void LenientNominationMappingRejectsMovesFromRetired()
    {
        var item = LenientNominationMapping.NewLenientNominationMapping("code-362", "name-109");
        item.Status = LenientNominationMappingStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientNominationMappingStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientNominationMappingStartsInCreated();
        LenientNominationMappingMovesToValidated();
        LenientNominationMappingRejectsMovesFromRetired();
    }
}
