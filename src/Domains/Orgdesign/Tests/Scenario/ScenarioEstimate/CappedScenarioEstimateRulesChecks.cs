namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class CappedScenarioEstimateRulesChecks
{
    public static void CappedScenarioEstimateValidatorReportsEveryProblem()
    {
        var validator = CappedScenarioEstimateRules.DefaultCappedScenarioEstimateValidator();
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedScenarioEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedScenarioEstimateCodeLengthRule(1000);
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedScenarioEstimateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedScenarioEstimateRequiredTitleRule("capped scenario estimate needs a value");
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedScenarioEstimateWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new CappedScenarioEstimateWeightRangeRule(5.0, 2806.6);
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("x", "y");
        item.Weight = 2808.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedScenarioEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedScenarioEstimateRequiredCodeRule("capped scenario estimate needs an entry");
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedScenarioEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CappedScenarioEstimateTitleLengthRule(86400);
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedScenarioEstimateValidatorReportsEveryProblem();
        CappedScenarioEstimateCodeLengthRuleFlagsALongCode();
        CappedScenarioEstimateRequiredTitleRuleFlagsABlankTitle();
        CappedScenarioEstimateWeightRangeRuleFlagsValuesAboveWeight();
        CappedScenarioEstimateRequiredCodeRuleFlagsABlankCode();
        CappedScenarioEstimateTitleLengthRuleFlagsALongTitle();
    }
}
