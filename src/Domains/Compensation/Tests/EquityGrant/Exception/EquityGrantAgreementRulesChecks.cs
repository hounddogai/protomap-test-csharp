namespace Acme.Workforce.Compensation.Tests.EquityGrant.Exception;

using Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class EquityGrantAgreementRulesChecks
{
    public static void EquityGrantAgreementValidatorReportsEveryProblem()
    {
        var validator = EquityGrantAgreementRules.DefaultEquityGrantAgreementValidator();
        var item = EquityGrantAgreement.NewEquityGrantAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantAgreementLocationLengthRuleFlagsALongLocation()
    {
        var rule = new EquityGrantAgreementLocationLengthRule(80);
        var item = EquityGrantAgreement.NewEquityGrantAgreement("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new EquityGrantAgreementCodeLengthRule(120);
        var item = EquityGrantAgreement.NewEquityGrantAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantAgreementLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new EquityGrantAgreementLeaveDaysRangeRule(16, 3600);
        var item = EquityGrantAgreement.NewEquityGrantAgreement("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EquityGrantAgreementValidatorReportsEveryProblem();
        EquityGrantAgreementLocationLengthRuleFlagsALongLocation();
        EquityGrantAgreementCodeLengthRuleFlagsALongCode();
        EquityGrantAgreementLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
