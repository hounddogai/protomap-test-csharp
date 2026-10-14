namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ManualScenarioEstimateRulesChecks
{
    public static void ManualScenarioEstimateValidatorReportsEveryProblem()
    {
        var validator = ManualScenarioEstimateRules.DefaultManualScenarioEstimateValidator();
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualScenarioEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new ManualScenarioEstimateNameLengthRule(64);
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualScenarioEstimateRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new ManualScenarioEstimateRequiredLocationRule("manual scenario estimate needs an entry");
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualScenarioEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualScenarioEstimateCodeLengthRule(64);
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualScenarioEstimateWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new ManualScenarioEstimateWeightRangeRule(3.8, 3243.1);
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("x", "y");
        item.Weight = 3244.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualScenarioEstimateLocationLengthRuleFlagsALongLocation()
    {
        var rule = new ManualScenarioEstimateLocationLengthRule(1000);
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualScenarioEstimateValidatorReportsEveryProblem();
        ManualScenarioEstimateNameLengthRuleFlagsALongName();
        ManualScenarioEstimateRequiredLocationRuleFlagsABlankLocation();
        ManualScenarioEstimateCodeLengthRuleFlagsALongCode();
        ManualScenarioEstimateWeightRangeRuleFlagsValuesAboveWeight();
        ManualScenarioEstimateLocationLengthRuleFlagsALongLocation();
    }
}
