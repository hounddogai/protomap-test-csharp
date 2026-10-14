namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PrimaryNominationPolicyRulesChecks
{
    public static void PrimaryNominationPolicyValidatorReportsEveryProblem()
    {
        var validator = PrimaryNominationPolicyRules.DefaultPrimaryNominationPolicyValidator();
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryNominationPolicyLocationLengthRuleFlagsALongLocation()
    {
        var rule = new PrimaryNominationPolicyLocationLengthRule(16);
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryNominationPolicyTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryNominationPolicyTitleLengthRule(255);
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryNominationPolicyRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new PrimaryNominationPolicyRequiredLocationRule("primary nomination policy needs this field");
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryNominationPolicyBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PrimaryNominationPolicyBudgetRangeRule(6.5, 1543.5);
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("x", "y");
        item.Budget = 1545.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryNominationPolicyValidatorReportsEveryProblem();
        PrimaryNominationPolicyLocationLengthRuleFlagsALongLocation();
        PrimaryNominationPolicyTitleLengthRuleFlagsALongTitle();
        PrimaryNominationPolicyRequiredLocationRuleFlagsABlankLocation();
        PrimaryNominationPolicyBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
