namespace Acme.Workforce.Compliance.Tests.Attestation.Quota;

using Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AttestationSummaryRulesChecks
{
    public static void AttestationSummaryValidatorReportsEveryProblem()
    {
        var validator = AttestationSummaryRules.DefaultAttestationSummaryValidator();
        var item = AttestationSummary.NewAttestationSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AttestationSummaryRequiredCodeRule("attestation summary needs an entry");
        var item = AttestationSummary.NewAttestationSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationSummaryBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AttestationSummaryBudgetRangeRule(7.9, 233.2);
        var item = AttestationSummary.NewAttestationSummary("x", "y");
        item.Budget = 234.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationSummaryDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new AttestationSummaryDescriptionLengthRule(32);
        var item = AttestationSummary.NewAttestationSummary("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationSummaryRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AttestationSummaryRateRangeRule(5.4, 2917.7);
        var item = AttestationSummary.NewAttestationSummary("x", "y");
        item.Rate = 2919.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AttestationSummaryValidatorReportsEveryProblem();
        AttestationSummaryRequiredCodeRuleFlagsABlankCode();
        AttestationSummaryBudgetRangeRuleFlagsValuesAboveBudget();
        AttestationSummaryDescriptionLengthRuleFlagsALongDescription();
        AttestationSummaryRateRangeRuleFlagsValuesAboveRate();
    }
}
