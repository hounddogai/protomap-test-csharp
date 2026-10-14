namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ManualSkillProfilePolicyChecks
{
    public static void ManualSkillProfilePolicyStartsInRequested()
    {
        var item = ManualSkillProfilePolicy.NewManualSkillProfilePolicy("code-891", "title-203");
        Check.Equal(item.Status, ManualSkillProfilePolicyStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualSkillProfilePolicyMovesToRevoked()
    {
        var item = ManualSkillProfilePolicy.NewManualSkillProfilePolicy("code-891", "title-203");
        try
        {
            item.MoveTo(ManualSkillProfilePolicyStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualSkillProfilePolicyStatus.Revoked, "status");
    }

    public static void ManualSkillProfilePolicyRejectsMovesFromExpired()
    {
        var item = ManualSkillProfilePolicy.NewManualSkillProfilePolicy("code-891", "title-203");
        item.Status = ManualSkillProfilePolicyStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualSkillProfilePolicyStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualSkillProfilePolicyStartsInRequested();
        ManualSkillProfilePolicyMovesToRevoked();
        ManualSkillProfilePolicyRejectsMovesFromExpired();
    }
}
