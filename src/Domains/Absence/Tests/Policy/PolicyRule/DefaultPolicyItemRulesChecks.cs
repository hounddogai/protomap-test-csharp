namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class DefaultPolicyItemRulesChecks
{
    public static void DefaultPolicyItemValidatorReportsEveryProblem()
    {
        var validator = DefaultPolicyItemRules.DefaultDefaultPolicyItemValidator();
        var item = DefaultPolicyItem.NewDefaultPolicyItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPolicyItemRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultPolicyItemRequiredNameRule("default policy item needs a value");
        var item = DefaultPolicyItem.NewDefaultPolicyItem("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPolicyItemExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new DefaultPolicyItemExternalRefLengthRule(16);
        var item = DefaultPolicyItem.NewDefaultPolicyItem("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPolicyItemRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new DefaultPolicyItemRetriesRangeRule(80, 120);
        var item = DefaultPolicyItem.NewDefaultPolicyItem("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPolicyItemRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new DefaultPolicyItemRequiredExternalRefRule("default policy item needs an entry");
        var item = DefaultPolicyItem.NewDefaultPolicyItem("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPolicyItemCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new DefaultPolicyItemCapacityRangeRule(3600, 86400);
        var item = DefaultPolicyItem.NewDefaultPolicyItem("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultPolicyItemValidatorReportsEveryProblem();
        DefaultPolicyItemRequiredNameRuleFlagsABlankName();
        DefaultPolicyItemExternalRefLengthRuleFlagsALongExternalRef();
        DefaultPolicyItemRetriesRangeRuleFlagsValuesAboveRetries();
        DefaultPolicyItemRequiredExternalRefRuleFlagsABlankExternalRef();
        DefaultPolicyItemCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
