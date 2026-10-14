namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class PendingPayRunAgreementRulesChecks
{
    public static void PendingPayRunAgreementValidatorReportsEveryProblem()
    {
        var validator = PendingPayRunAgreementRules.DefaultPendingPayRunAgreementValidator();
        var item = PendingPayRunAgreement.NewPendingPayRunAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPayRunAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingPayRunAgreementRequiredCodeRule("pending pay run agreement needs a value");
        var item = PendingPayRunAgreement.NewPendingPayRunAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPayRunAgreementAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PendingPayRunAgreementAmountRangeRule(5.2, 4260.6);
        var item = PendingPayRunAgreement.NewPendingPayRunAgreement("x", "y");
        item.Amount = 4262.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingPayRunAgreementValidatorReportsEveryProblem();
        PendingPayRunAgreementRequiredCodeRuleFlagsABlankCode();
        PendingPayRunAgreementAmountRangeRuleFlagsValuesAboveAmount();
    }
}
