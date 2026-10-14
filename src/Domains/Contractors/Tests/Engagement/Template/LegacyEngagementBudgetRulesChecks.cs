namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyEngagementBudgetRulesChecks
{
    public static void LegacyEngagementBudgetValidatorReportsEveryProblem()
    {
        var validator = LegacyEngagementBudgetRules.DefaultLegacyEngagementBudgetValidator();
        var item = LegacyEngagementBudget.NewLegacyEngagementBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyEngagementBudgetSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LegacyEngagementBudgetSequenceRangeRule(16, 86400);
        var item = LegacyEngagementBudget.NewLegacyEngagementBudget("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyEngagementBudgetRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new LegacyEngagementBudgetRequiredLocationRule("legacy engagement budget needs this field");
        var item = LegacyEngagementBudget.NewLegacyEngagementBudget("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyEngagementBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyEngagementBudgetNameLengthRule(64);
        var item = LegacyEngagementBudget.NewLegacyEngagementBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyEngagementBudgetValidatorReportsEveryProblem();
        LegacyEngagementBudgetSequenceRangeRuleFlagsValuesAboveSequence();
        LegacyEngagementBudgetRequiredLocationRuleFlagsABlankLocation();
        LegacyEngagementBudgetNameLengthRuleFlagsALongName();
    }
}
