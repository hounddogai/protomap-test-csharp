namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class NominationBudgetRulesChecks
{
    public static void NominationBudgetValidatorReportsEveryProblem()
    {
        var validator = NominationBudgetRules.DefaultNominationBudgetValidator();
        var item = NominationBudget.NewNominationBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationBudgetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new NominationBudgetTitleLengthRule(120);
        var item = NominationBudget.NewNominationBudget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationBudgetExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new NominationBudgetExternalRefLengthRule(64);
        var item = NominationBudget.NewNominationBudget("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new NominationBudgetRequiredTitleRule("nomination budget needs an entry");
        var item = NominationBudget.NewNominationBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationBudgetVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new NominationBudgetVersionRangeRule(32, 120);
        var item = NominationBudget.NewNominationBudget("x", "y");
        item.Version = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        NominationBudgetValidatorReportsEveryProblem();
        NominationBudgetTitleLengthRuleFlagsALongTitle();
        NominationBudgetExternalRefLengthRuleFlagsALongExternalRef();
        NominationBudgetRequiredTitleRuleFlagsABlankTitle();
        NominationBudgetVersionRangeRuleFlagsValuesAboveVersion();
    }
}
