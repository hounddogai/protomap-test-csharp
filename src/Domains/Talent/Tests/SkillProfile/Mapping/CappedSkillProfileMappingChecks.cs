namespace Acme.Workforce.Talent.Tests.SkillProfile.Mapping;

using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class CappedSkillProfileMappingChecks
{
    public static void CappedSkillProfileMappingStartsInPlanned()
    {
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("code-284", "title-401");
        Check.Equal(item.Status, CappedSkillProfileMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSkillProfileMappingMovesToScheduled()
    {
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("code-284", "title-401");
        try
        {
            item.MoveTo(CappedSkillProfileMappingStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSkillProfileMappingStatus.Scheduled, "status");
    }

    public static void CappedSkillProfileMappingRejectsMovesFromCancelled()
    {
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("code-284", "title-401");
        item.Status = CappedSkillProfileMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedSkillProfileMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedSkillProfileMappingStartsInPlanned();
        CappedSkillProfileMappingMovesToScheduled();
        CappedSkillProfileMappingRejectsMovesFromCancelled();
    }
}
