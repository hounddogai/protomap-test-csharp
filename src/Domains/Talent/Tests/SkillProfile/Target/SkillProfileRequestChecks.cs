namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfileRequestChecks
{
    public static void SkillProfileRequestStartsInCreated()
    {
        var item = SkillProfileRequest.NewSkillProfileRequest("code-538", "name-873");
        Check.Equal(item.Status, SkillProfileRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileRequestMovesToValidated()
    {
        var item = SkillProfileRequest.NewSkillProfileRequest("code-538", "name-873");
        try
        {
            item.MoveTo(SkillProfileRequestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileRequestStatus.Validated, "status");
    }

    public static void SkillProfileRequestRejectsMovesFromRetired()
    {
        var item = SkillProfileRequest.NewSkillProfileRequest("code-538", "name-873");
        item.Status = SkillProfileRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(SkillProfileRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        SkillProfileRequestStartsInCreated();
        SkillProfileRequestMovesToValidated();
        SkillProfileRequestRejectsMovesFromRetired();
    }
}
