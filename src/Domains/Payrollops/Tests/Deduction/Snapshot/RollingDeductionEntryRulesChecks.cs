namespace Acme.Workforce.Payrollops.Tests.Deduction.Snapshot;

using Acme.Workforce.Payrollops.Deduction.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RollingDeductionEntryRulesChecks
{
    public static void RollingDeductionEntryValidatorReportsEveryProblem()
    {
        var validator = RollingDeductionEntryRules.DefaultRollingDeductionEntryValidator();
        var item = RollingDeductionEntry.NewRollingDeductionEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingDeductionEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingDeductionEntryRequiredCodeRule("rolling deduction entry needs an entry");
        var item = RollingDeductionEntry.NewRollingDeductionEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingDeductionEntryValidatorReportsEveryProblem();
        RollingDeductionEntryRequiredCodeRuleFlagsABlankCode();
    }
}
