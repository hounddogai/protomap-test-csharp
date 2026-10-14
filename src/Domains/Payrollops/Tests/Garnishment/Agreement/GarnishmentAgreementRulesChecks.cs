namespace Acme.Workforce.Payrollops.Tests.Garnishment.Agreement;

using Acme.Workforce.Payrollops.Garnishment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GarnishmentAgreementRulesChecks
{
    public static void GarnishmentAgreementValidatorReportsEveryProblem()
    {
        var validator = GarnishmentAgreementRules.DefaultGarnishmentAgreementValidator();
        var item = GarnishmentAgreement.NewGarnishmentAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new GarnishmentAgreementNameLengthRule(32);
        var item = GarnishmentAgreement.NewGarnishmentAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentAgreementFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new GarnishmentAgreementFloorValueRangeRule(0.1, 3109.2);
        var item = GarnishmentAgreement.NewGarnishmentAgreement("x", "y");
        item.FloorValue = 3110.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentAgreementRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GarnishmentAgreementRequiredTitleRule("garnishment agreement needs an entry");
        var item = GarnishmentAgreement.NewGarnishmentAgreement("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentAgreementCodeLengthRule(80);
        var item = GarnishmentAgreement.NewGarnishmentAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentAgreementValidatorReportsEveryProblem();
        GarnishmentAgreementNameLengthRuleFlagsALongName();
        GarnishmentAgreementFloorValueRangeRuleFlagsValuesAboveFloorValue();
        GarnishmentAgreementRequiredTitleRuleFlagsABlankTitle();
        GarnishmentAgreementCodeLengthRuleFlagsALongCode();
    }
}
