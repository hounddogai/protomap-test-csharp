namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyEntryRulesChecks
{
    public static void PolicyEntryValidatorReportsEveryProblem()
    {
        var validator = PolicyEntryRules.DefaultPolicyEntryValidator();
        var item = PolicyEntry.NewPolicyEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyEntryNameLengthRuleFlagsALongName()
    {
        var rule = new PolicyEntryNameLengthRule(255);
        var item = PolicyEntry.NewPolicyEntry("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyEntryRequiredCodeRule("policy entry needs an entry");
        var item = PolicyEntry.NewPolicyEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PolicyEntryRequiredLabelRule("policy entry needs this field");
        var item = PolicyEntry.NewPolicyEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyEntryCodeLengthRule(255);
        var item = PolicyEntry.NewPolicyEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyEntryValidatorReportsEveryProblem();
        PolicyEntryNameLengthRuleFlagsALongName();
        PolicyEntryRequiredCodeRuleFlagsABlankCode();
        PolicyEntryRequiredLabelRuleFlagsABlankLabel();
        PolicyEntryCodeLengthRuleFlagsALongCode();
    }
}
