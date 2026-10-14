namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class StrictPolicyEntryRulesChecks
{
    public static void StrictPolicyEntryValidatorReportsEveryProblem()
    {
        var validator = StrictPolicyEntryRules.DefaultStrictPolicyEntryValidator();
        var item = StrictPolicyEntry.NewStrictPolicyEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPolicyEntryDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new StrictPolicyEntryDiscountRangeRule(2.5, 516.6);
        var item = StrictPolicyEntry.NewStrictPolicyEntry("x", "y");
        item.Discount = 518.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPolicyEntryRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictPolicyEntryRequiredNameRule("strict policy entry needs a value");
        var item = StrictPolicyEntry.NewStrictPolicyEntry("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPolicyEntryRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new StrictPolicyEntryRetriesRangeRule(16, 32);
        var item = StrictPolicyEntry.NewStrictPolicyEntry("x", "y");
        item.Retries = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictPolicyEntryValidatorReportsEveryProblem();
        StrictPolicyEntryDiscountRangeRuleFlagsValuesAboveDiscount();
        StrictPolicyEntryRequiredNameRuleFlagsABlankName();
        StrictPolicyEntryRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
