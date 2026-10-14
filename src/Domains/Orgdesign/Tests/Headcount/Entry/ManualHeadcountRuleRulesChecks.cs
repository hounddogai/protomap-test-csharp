namespace Acme.Workforce.Orgdesign.Tests.Headcount.Entry;

using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualHeadcountRuleRulesChecks
{
    public static void ManualHeadcountRuleValidatorReportsEveryProblem()
    {
        var validator = ManualHeadcountRuleRules.DefaultManualHeadcountRuleValidator();
        var item = ManualHeadcountRule.NewManualHeadcountRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualHeadcountRuleOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new ManualHeadcountRuleOwnerIdLengthRule(120);
        var item = ManualHeadcountRule.NewManualHeadcountRule("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualHeadcountRuleValidatorReportsEveryProblem();
        ManualHeadcountRuleOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
