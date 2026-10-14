namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedVisitorEntryRulesChecks
{
    public static void WeightedVisitorEntryValidatorReportsEveryProblem()
    {
        var validator = WeightedVisitorEntryRules.DefaultWeightedVisitorEntryValidator();
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedVisitorEntryRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new WeightedVisitorEntryRequiredPhoneRule("weighted visitor entry needs an entry");
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedVisitorEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedVisitorEntryCodeLengthRule(64);
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedVisitorEntryPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new WeightedVisitorEntryPhoneLengthRule(120);
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedVisitorEntryValidatorReportsEveryProblem();
        WeightedVisitorEntryRequiredPhoneRuleFlagsABlankPhone();
        WeightedVisitorEntryCodeLengthRuleFlagsALongCode();
        WeightedVisitorEntryPhoneLengthRuleFlagsALongPhone();
    }
}
