namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalSkillProfileRecordRulesChecks
{
    public static void RegionalSkillProfileRecordValidatorReportsEveryProblem()
    {
        var validator = RegionalSkillProfileRecordRules.DefaultRegionalSkillProfileRecordValidator();
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSkillProfileRecordFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new RegionalSkillProfileRecordFloorValueRangeRule(9.7, 4456.3);
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("x", "y");
        item.FloorValue = 4457.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSkillProfileRecordWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new RegionalSkillProfileRecordWindowDaysRangeRule(64, 1000);
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSkillProfileRecordRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new RegionalSkillProfileRecordRequiredChecksumRule("regional skill profile record needs an entry");
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSkillProfileRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalSkillProfileRecordLabelLengthRule(16);
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalSkillProfileRecordValidatorReportsEveryProblem();
        RegionalSkillProfileRecordFloorValueRangeRuleFlagsValuesAboveFloorValue();
        RegionalSkillProfileRecordWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        RegionalSkillProfileRecordRequiredChecksumRuleFlagsABlankChecksum();
        RegionalSkillProfileRecordLabelLengthRuleFlagsALongLabel();
    }
}
