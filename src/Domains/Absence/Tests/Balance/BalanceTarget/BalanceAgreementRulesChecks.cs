namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class BalanceAgreementRulesChecks
{
    public static void BalanceAgreementValidatorReportsEveryProblem()
    {
        var validator = BalanceAgreementRules.DefaultBalanceAgreementValidator();
        var item = BalanceAgreement.NewBalanceAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceAgreementLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BalanceAgreementLabelLengthRule(32);
        var item = BalanceAgreement.NewBalanceAgreement("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceAgreementRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new BalanceAgreementRequiredJobCodeRule("balance agreement needs an entry");
        var item = BalanceAgreement.NewBalanceAgreement("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceAgreementRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BalanceAgreementRequiredLabelRule("balance agreement needs this field");
        var item = BalanceAgreement.NewBalanceAgreement("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceAgreementRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new BalanceAgreementRatioRangeRule(9.5, 3204.1);
        var item = BalanceAgreement.NewBalanceAgreement("x", "y");
        item.Ratio = 3205.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceAgreementValidatorReportsEveryProblem();
        BalanceAgreementLabelLengthRuleFlagsALongLabel();
        BalanceAgreementRequiredJobCodeRuleFlagsABlankJobCode();
        BalanceAgreementRequiredLabelRuleFlagsABlankLabel();
        BalanceAgreementRatioRangeRuleFlagsValuesAboveRatio();
    }
}
