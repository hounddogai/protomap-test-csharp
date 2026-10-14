namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PrimaryNominationMappingChecks
{
    public static void PrimaryNominationMappingStartsInNew()
    {
        var item = PrimaryNominationMapping.NewPrimaryNominationMapping("code-736", "name-262");
        Check.Equal(item.Status, PrimaryNominationMappingStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryNominationMappingMovesToFailed()
    {
        var item = PrimaryNominationMapping.NewPrimaryNominationMapping("code-736", "name-262");
        try
        {
            item.MoveTo(PrimaryNominationMappingStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryNominationMappingStatus.Failed, "status");
    }

    public static void PrimaryNominationMappingRejectsMovesFromFailed()
    {
        var item = PrimaryNominationMapping.NewPrimaryNominationMapping("code-736", "name-262");
        item.Status = PrimaryNominationMappingStatus.Failed;
        Check.Fails(() => item.MoveTo(PrimaryNominationMappingStatus.New), "move");
    }

    public static void RunAll()
    {
        PrimaryNominationMappingStartsInNew();
        PrimaryNominationMappingMovesToFailed();
        PrimaryNominationMappingRejectsMovesFromFailed();
    }
}
