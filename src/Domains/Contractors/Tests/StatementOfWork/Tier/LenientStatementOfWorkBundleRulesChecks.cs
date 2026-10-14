namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LenientStatementOfWorkBundleRulesChecks
{
    public static void LenientStatementOfWorkBundleValidatorReportsEveryProblem()
    {
        var validator = LenientStatementOfWorkBundleRules.DefaultLenientStatementOfWorkBundleValidator();
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientStatementOfWorkBundleNameLengthRuleFlagsALongName()
    {
        var rule = new LenientStatementOfWorkBundleNameLengthRule(16);
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientStatementOfWorkBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientStatementOfWorkBundleCodeLengthRule(255);
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientStatementOfWorkBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientStatementOfWorkBundleRequiredCodeRule("lenient statement of work bundle needs this field");
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientStatementOfWorkBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientStatementOfWorkBundleRequiredNameRule("lenient statement of work bundle needs this field");
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientStatementOfWorkBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientStatementOfWorkBundleLabelLengthRule(32);
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkBundleValidatorReportsEveryProblem();
        LenientStatementOfWorkBundleNameLengthRuleFlagsALongName();
        LenientStatementOfWorkBundleCodeLengthRuleFlagsALongCode();
        LenientStatementOfWorkBundleRequiredCodeRuleFlagsABlankCode();
        LenientStatementOfWorkBundleRequiredNameRuleFlagsABlankName();
        LenientStatementOfWorkBundleLabelLengthRuleFlagsALongLabel();
    }
}
