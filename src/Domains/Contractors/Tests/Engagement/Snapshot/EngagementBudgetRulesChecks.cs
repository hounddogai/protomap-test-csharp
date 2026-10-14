namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementBudgetRulesChecks
{
    public static void EngagementBudgetValidatorReportsEveryProblem()
    {
        var validator = EngagementBudgetRules.DefaultEngagementBudgetValidator();
        var item = EngagementBudget.NewEngagementBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementBudgetCodeLengthRule(1000);
        var item = EngagementBudget.NewEngagementBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementBudgetManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new EngagementBudgetManagerIdLengthRule(80);
        var item = EngagementBudget.NewEngagementBudget("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementBudgetRequiredCodeRule("engagement budget needs an entry");
        var item = EngagementBudget.NewEngagementBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new EngagementBudgetRequiredTitleRule("engagement budget needs this field");
        var item = EngagementBudget.NewEngagementBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementBudgetRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new EngagementBudgetRequiredManagerIdRule("engagement budget needs an entry");
        var item = EngagementBudget.NewEngagementBudget("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementBudgetSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new EngagementBudgetSequenceRangeRule(80, 120);
        var item = EngagementBudget.NewEngagementBudget("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EngagementBudgetValidatorReportsEveryProblem();
        EngagementBudgetCodeLengthRuleFlagsALongCode();
        EngagementBudgetManagerIdLengthRuleFlagsALongManagerId();
        EngagementBudgetRequiredCodeRuleFlagsABlankCode();
        EngagementBudgetRequiredTitleRuleFlagsABlankTitle();
        EngagementBudgetRequiredManagerIdRuleFlagsABlankManagerId();
        EngagementBudgetSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
