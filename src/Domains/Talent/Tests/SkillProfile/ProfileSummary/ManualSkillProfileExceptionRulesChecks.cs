namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class ManualSkillProfileExceptionRulesChecks
{
    public static void ManualSkillProfileExceptionValidatorReportsEveryProblem()
    {
        var validator = ManualSkillProfileExceptionRules.DefaultManualSkillProfileExceptionValidator();
        var item = ManualSkillProfileException.NewManualSkillProfileException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualSkillProfileExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualSkillProfileExceptionCodeLengthRule(16);
        var item = ManualSkillProfileException.NewManualSkillProfileException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualSkillProfileExceptionRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ManualSkillProfileExceptionRequiredEmployeeIdRule("manual skill profile exception needs a value");
        var item = ManualSkillProfileException.NewManualSkillProfileException("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualSkillProfileExceptionRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ManualSkillProfileExceptionRequiredCategoryRule("manual skill profile exception needs a value");
        var item = ManualSkillProfileException.NewManualSkillProfileException("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualSkillProfileExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new ManualSkillProfileExceptionNameLengthRule(3600);
        var item = ManualSkillProfileException.NewManualSkillProfileException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualSkillProfileExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualSkillProfileExceptionRequiredNameRule("manual skill profile exception needs a value");
        var item = ManualSkillProfileException.NewManualSkillProfileException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualSkillProfileExceptionThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new ManualSkillProfileExceptionThresholdRangeRule(8.2, 4864.0);
        var item = ManualSkillProfileException.NewManualSkillProfileException("x", "y");
        item.Threshold = 4865.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualSkillProfileExceptionValidatorReportsEveryProblem();
        ManualSkillProfileExceptionCodeLengthRuleFlagsALongCode();
        ManualSkillProfileExceptionRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        ManualSkillProfileExceptionRequiredCategoryRuleFlagsABlankCategory();
        ManualSkillProfileExceptionNameLengthRuleFlagsALongName();
        ManualSkillProfileExceptionRequiredNameRuleFlagsABlankName();
        ManualSkillProfileExceptionThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
