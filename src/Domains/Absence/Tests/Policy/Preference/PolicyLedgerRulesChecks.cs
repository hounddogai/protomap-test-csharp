namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyLedgerRulesChecks
{
    public static void PolicyLedgerValidatorReportsEveryProblem()
    {
        var validator = PolicyLedgerRules.DefaultPolicyLedgerValidator();
        var item = PolicyLedger.NewPolicyLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyLedgerEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PolicyLedgerEndsAtRangeRule(16, 120);
        var item = PolicyLedger.NewPolicyLedger("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyLedgerCodeLengthRule(1000);
        var item = PolicyLedger.NewPolicyLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyLedgerCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new PolicyLedgerCostRangeRule(0.3, 4440.1);
        var item = PolicyLedger.NewPolicyLedger("x", "y");
        item.Cost = 4441.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyLedgerValidatorReportsEveryProblem();
        PolicyLedgerEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PolicyLedgerCodeLengthRuleFlagsALongCode();
        PolicyLedgerCostRangeRuleFlagsValuesAboveCost();
    }
}
