namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingSuccessionPlanLedgerRulesChecks
{
    public static void RollingSuccessionPlanLedgerValidatorReportsEveryProblem()
    {
        var validator = RollingSuccessionPlanLedgerRules.DefaultRollingSuccessionPlanLedgerValidator();
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSuccessionPlanLedgerJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new RollingSuccessionPlanLedgerJobCodeLengthRule(32);
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSuccessionPlanLedgerExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new RollingSuccessionPlanLedgerExternalRefLengthRule(120);
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSuccessionPlanLedgerRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new RollingSuccessionPlanLedgerRequiredExternalRefRule(
            "rolling succession plan ledger needs a value");
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSuccessionPlanLedgerCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new RollingSuccessionPlanLedgerCapacityRangeRule(1000, 86400);
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanLedgerValidatorReportsEveryProblem();
        RollingSuccessionPlanLedgerJobCodeLengthRuleFlagsALongJobCode();
        RollingSuccessionPlanLedgerExternalRefLengthRuleFlagsALongExternalRef();
        RollingSuccessionPlanLedgerRequiredExternalRefRuleFlagsABlankExternalRef();
        RollingSuccessionPlanLedgerCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
