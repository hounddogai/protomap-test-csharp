namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalSkillProfileRecordChecks
{
    public static void RegionalSkillProfileRecordStartsInNew()
    {
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("code-250", "label-318");
        Check.Equal(item.Status, RegionalSkillProfileRecordStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSkillProfileRecordMovesToQueued()
    {
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("code-250", "label-318");
        try
        {
            item.MoveTo(RegionalSkillProfileRecordStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSkillProfileRecordStatus.Queued, "status");
    }

    public static void RegionalSkillProfileRecordRejectsMovesFromFailed()
    {
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("code-250", "label-318");
        item.Status = RegionalSkillProfileRecordStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalSkillProfileRecordStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalSkillProfileRecordStartsInNew();
        RegionalSkillProfileRecordMovesToQueued();
        RegionalSkillProfileRecordRejectsMovesFromFailed();
    }
}
