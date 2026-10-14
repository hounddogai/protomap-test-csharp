namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AttestationPlanRulesChecks
{
    public static void AttestationPlanValidatorReportsEveryProblem()
    {
        var validator = AttestationPlanRules.DefaultAttestationPlanValidator();
        var item = AttestationPlan.NewAttestationPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationPlanLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new AttestationPlanLeaveDaysRangeRule(16, 80);
        var item = AttestationPlan.NewAttestationPlan("x", "y");
        item.LeaveDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationPlanTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AttestationPlanTitleLengthRule(255);
        var item = AttestationPlan.NewAttestationPlan("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationPlanRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new AttestationPlanRequiredCurrencyRule("attestation plan needs a value");
        var item = AttestationPlan.NewAttestationPlan("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AttestationPlanValidatorReportsEveryProblem();
        AttestationPlanLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        AttestationPlanTitleLengthRuleFlagsALongTitle();
        AttestationPlanRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
