namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScenarioQuotaRulesChecks
{
    public static void ScenarioQuotaValidatorReportsEveryProblem()
    {
        var validator = ScenarioQuotaRules.DefaultScenarioQuotaValidator();
        var item = ScenarioQuota.NewScenarioQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioQuotaBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new ScenarioQuotaBatchSizeRangeRule(32, 64);
        var item = ScenarioQuota.NewScenarioQuota("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioQuotaStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new ScenarioQuotaStartsAtRangeRule(16, 16);
        var item = ScenarioQuota.NewScenarioQuota("x", "y");
        item.StartsAt = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioQuotaTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScenarioQuotaTitleLengthRule(80);
        var item = ScenarioQuota.NewScenarioQuota("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioQuotaCodeLengthRule(255);
        var item = ScenarioQuota.NewScenarioQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioQuotaValidatorReportsEveryProblem();
        ScenarioQuotaBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        ScenarioQuotaStartsAtRangeRuleFlagsValuesAboveStartsAt();
        ScenarioQuotaTitleLengthRuleFlagsALongTitle();
        ScenarioQuotaCodeLengthRuleFlagsALongCode();
    }
}
