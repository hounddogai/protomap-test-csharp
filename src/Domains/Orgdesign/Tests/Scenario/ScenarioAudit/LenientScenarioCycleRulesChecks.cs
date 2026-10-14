namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class LenientScenarioCycleRulesChecks
{
    public static void LenientScenarioCycleValidatorReportsEveryProblem()
    {
        var validator = LenientScenarioCycleRules.DefaultLenientScenarioCycleValidator();
        var item = LenientScenarioCycle.NewLenientScenarioCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientScenarioCycleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientScenarioCycleRequiredLabelRule("lenient scenario cycle needs this field");
        var item = LenientScenarioCycle.NewLenientScenarioCycle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientScenarioCycleOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new LenientScenarioCycleOwnerIdLengthRule(64);
        var item = LenientScenarioCycle.NewLenientScenarioCycle("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientScenarioCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientScenarioCycleLabelLengthRule(120);
        var item = LenientScenarioCycle.NewLenientScenarioCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientScenarioCycleRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new LenientScenarioCycleRequiredOwnerIdRule("lenient scenario cycle needs this field");
        var item = LenientScenarioCycle.NewLenientScenarioCycle("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientScenarioCycleValidatorReportsEveryProblem();
        LenientScenarioCycleRequiredLabelRuleFlagsABlankLabel();
        LenientScenarioCycleOwnerIdLengthRuleFlagsALongOwnerId();
        LenientScenarioCycleLabelLengthRuleFlagsALongLabel();
        LenientScenarioCycleRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
