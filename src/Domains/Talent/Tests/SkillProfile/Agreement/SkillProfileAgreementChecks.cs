namespace Acme.Workforce.Talent.Tests.SkillProfile.Agreement;

using Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SkillProfileAgreementChecks
{
    public static void SkillProfileAgreementStartsInIdle()
    {
        var item = SkillProfileAgreement.NewSkillProfileAgreement("code-896", "title-278");
        Check.Equal(item.Status, SkillProfileAgreementStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileAgreementMovesToWarming()
    {
        var item = SkillProfileAgreement.NewSkillProfileAgreement("code-896", "title-278");
        try
        {
            item.MoveTo(SkillProfileAgreementStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileAgreementStatus.Warming, "status");
    }

    public static void SkillProfileAgreementRejectsMovesFromStopped()
    {
        var item = SkillProfileAgreement.NewSkillProfileAgreement("code-896", "title-278");
        item.Status = SkillProfileAgreementStatus.Stopped;
        Check.Fails(() => item.MoveTo(SkillProfileAgreementStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SkillProfileAgreementStartsInIdle();
        SkillProfileAgreementMovesToWarming();
        SkillProfileAgreementRejectsMovesFromStopped();
    }
}
