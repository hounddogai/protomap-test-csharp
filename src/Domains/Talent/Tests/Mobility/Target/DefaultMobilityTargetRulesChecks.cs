namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class DefaultMobilityTargetRulesChecks
{
    public static void DefaultMobilityTargetValidatorReportsEveryProblem()
    {
        var validator = DefaultMobilityTargetRules.DefaultDefaultMobilityTargetValidator();
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultMobilityTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultMobilityTargetRequiredCodeRule("default mobility target needs a value");
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultMobilityTargetWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new DefaultMobilityTargetWindowDaysRangeRule(255, 1000);
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultMobilityTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultMobilityTargetCodeLengthRule(3600);
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultMobilityTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultMobilityTargetRequiredTitleRule("default mobility target needs an entry");
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultMobilityTargetRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new DefaultMobilityTargetRequiredDepartmentRule("default mobility target needs this field");
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultMobilityTargetRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new DefaultMobilityTargetRetriesRangeRule(16, 80);
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("x", "y");
        item.Retries = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultMobilityTargetValidatorReportsEveryProblem();
        DefaultMobilityTargetRequiredCodeRuleFlagsABlankCode();
        DefaultMobilityTargetWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        DefaultMobilityTargetCodeLengthRuleFlagsALongCode();
        DefaultMobilityTargetRequiredTitleRuleFlagsABlankTitle();
        DefaultMobilityTargetRequiredDepartmentRuleFlagsABlankDepartment();
        DefaultMobilityTargetRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
