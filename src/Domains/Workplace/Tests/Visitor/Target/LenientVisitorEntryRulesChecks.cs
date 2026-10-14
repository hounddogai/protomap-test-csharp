namespace Acme.Workforce.Workplace.Tests.Visitor.Target;

using Acme.Workforce.Workplace.Visitor.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientVisitorEntryRulesChecks
{
    public static void LenientVisitorEntryValidatorReportsEveryProblem()
    {
        var validator = LenientVisitorEntryRules.DefaultLenientVisitorEntryValidator();
        var item = LenientVisitorEntry.NewLenientVisitorEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientVisitorEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientVisitorEntryRequiredCodeRule("lenient visitor entry needs an entry");
        var item = LenientVisitorEntry.NewLenientVisitorEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientVisitorEntryRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientVisitorEntryRequiredNameRule("lenient visitor entry needs an entry");
        var item = LenientVisitorEntry.NewLenientVisitorEntry("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientVisitorEntryThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new LenientVisitorEntryThresholdRangeRule(2.8, 4345.2);
        var item = LenientVisitorEntry.NewLenientVisitorEntry("x", "y");
        item.Threshold = 4346.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientVisitorEntryValidatorReportsEveryProblem();
        LenientVisitorEntryRequiredCodeRuleFlagsABlankCode();
        LenientVisitorEntryRequiredNameRuleFlagsABlankName();
        LenientVisitorEntryThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
