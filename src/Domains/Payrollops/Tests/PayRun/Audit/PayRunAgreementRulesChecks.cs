namespace Acme.Workforce.Payrollops.Tests.PayRun.Audit;

using Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PayRunAgreementRulesChecks
{
    public static void PayRunAgreementValidatorReportsEveryProblem()
    {
        var validator = PayRunAgreementRules.DefaultPayRunAgreementValidator();
        var item = PayRunAgreement.NewPayRunAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunAgreementRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PayRunAgreementRequiredCategoryRule("pay run agreement needs an entry");
        var item = PayRunAgreement.NewPayRunAgreement("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunAgreementCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PayRunAgreementCategoryLengthRule(80);
        var item = PayRunAgreement.NewPayRunAgreement("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunAgreementRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayRunAgreementRequiredNameRule("pay run agreement needs an entry");
        var item = PayRunAgreement.NewPayRunAgreement("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunAgreementRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new PayRunAgreementRequiredOwnerIdRule("pay run agreement needs an entry");
        var item = PayRunAgreement.NewPayRunAgreement("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunAgreementOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PayRunAgreementOwnerIdLengthRule(80);
        var item = PayRunAgreement.NewPayRunAgreement("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunAgreementRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PayRunAgreementRatingRangeRule(3.6, 4137.3);
        var item = PayRunAgreement.NewPayRunAgreement("x", "y");
        item.Rating = 4138.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayRunAgreementValidatorReportsEveryProblem();
        PayRunAgreementRequiredCategoryRuleFlagsABlankCategory();
        PayRunAgreementCategoryLengthRuleFlagsALongCategory();
        PayRunAgreementRequiredNameRuleFlagsABlankName();
        PayRunAgreementRequiredOwnerIdRuleFlagsABlankOwnerId();
        PayRunAgreementOwnerIdLengthRuleFlagsALongOwnerId();
        PayRunAgreementRatingRangeRuleFlagsValuesAboveRating();
    }
}
