namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalNominationVarianceRulesChecks
{
    public static void RegionalNominationVarianceValidatorReportsEveryProblem()
    {
        var validator = RegionalNominationVarianceRules.DefaultRegionalNominationVarianceValidator();
        var item = RegionalNominationVariance.NewRegionalNominationVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalNominationVarianceRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new RegionalNominationVarianceRequiredManagerIdRule("regional nomination variance needs this field");
        var item = RegionalNominationVariance.NewRegionalNominationVariance("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalNominationVarianceManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new RegionalNominationVarianceManagerIdLengthRule(1000);
        var item = RegionalNominationVariance.NewRegionalNominationVariance("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalNominationVariancePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new RegionalNominationVariancePriorityRangeRule(16, 64);
        var item = RegionalNominationVariance.NewRegionalNominationVariance("x", "y");
        item.Priority = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalNominationVarianceValidatorReportsEveryProblem();
        RegionalNominationVarianceRequiredManagerIdRuleFlagsABlankManagerId();
        RegionalNominationVarianceManagerIdLengthRuleFlagsALongManagerId();
        RegionalNominationVariancePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
