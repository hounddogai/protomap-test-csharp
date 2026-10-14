namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class OffboardingMappingChecks
{
    public static void OffboardingMappingStartsInOpen()
    {
        var item = OffboardingMapping.NewOffboardingMapping("code-647", "title-205");
        Check.Equal(item.Status, OffboardingMappingStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingMappingMovesToArchived()
    {
        var item = OffboardingMapping.NewOffboardingMapping("code-647", "title-205");
        try
        {
            item.MoveTo(OffboardingMappingStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingMappingStatus.Archived, "status");
    }

    public static void OffboardingMappingRejectsMovesFromArchived()
    {
        var item = OffboardingMapping.NewOffboardingMapping("code-647", "title-205");
        item.Status = OffboardingMappingStatus.Archived;
        Check.Fails(() => item.MoveTo(OffboardingMappingStatus.Open), "move");
    }

    public static void RunAll()
    {
        OffboardingMappingStartsInOpen();
        OffboardingMappingMovesToArchived();
        OffboardingMappingRejectsMovesFromArchived();
    }
}
