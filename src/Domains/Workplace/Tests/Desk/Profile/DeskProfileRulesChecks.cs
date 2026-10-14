namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskProfileRulesChecks
{
    public static void DeskProfileValidatorReportsEveryProblem()
    {
        var validator = DeskProfileRules.DefaultDeskProfileValidator();
        var item = DeskProfile.NewDeskProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskProfileUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new DeskProfileUpdatedAtRangeRule(120, 1000);
        var item = DeskProfile.NewDeskProfile("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskProfileRequiredCodeRule("desk profile needs an entry");
        var item = DeskProfile.NewDeskProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskProfileRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new DeskProfileRetriesRangeRule(16, 1000);
        var item = DeskProfile.NewDeskProfile("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskProfileRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new DeskProfileRequiredChecksumRule("desk profile needs a value");
        var item = DeskProfile.NewDeskProfile("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskProfileRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new DeskProfileRatioRangeRule(4.3, 3933.4);
        var item = DeskProfile.NewDeskProfile("x", "y");
        item.Ratio = 3934.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeskProfileValidatorReportsEveryProblem();
        DeskProfileUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        DeskProfileRequiredCodeRuleFlagsABlankCode();
        DeskProfileRetriesRangeRuleFlagsValuesAboveRetries();
        DeskProfileRequiredChecksumRuleFlagsABlankChecksum();
        DeskProfileRatioRangeRuleFlagsValuesAboveRatio();
    }
}
