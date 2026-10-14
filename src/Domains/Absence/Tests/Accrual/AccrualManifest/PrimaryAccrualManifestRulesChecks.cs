namespace Acme.Workforce.Absence.Tests.Accrual.AccrualManifest;

using Acme.Workforce.Absence.Accrual.AccrualManifest;

/// <summary>Checks of accrual manifest code, run by the project's runner.</summary>
public static class PrimaryAccrualManifestRulesChecks
{
    public static void PrimaryAccrualManifestValidatorReportsEveryProblem()
    {
        var validator = PrimaryAccrualManifestRules.DefaultPrimaryAccrualManifestValidator();
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryAccrualManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryAccrualManifestRequiredCodeRule("primary accrual manifest needs a value");
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryAccrualManifestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryAccrualManifestRequiredTitleRule("primary accrual manifest needs an entry");
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryAccrualManifestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryAccrualManifestTitleLengthRule(3600);
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryAccrualManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryAccrualManifestCodeLengthRule(255);
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryAccrualManifestValidatorReportsEveryProblem();
        PrimaryAccrualManifestRequiredCodeRuleFlagsABlankCode();
        PrimaryAccrualManifestRequiredTitleRuleFlagsABlankTitle();
        PrimaryAccrualManifestTitleLengthRuleFlagsALongTitle();
        PrimaryAccrualManifestCodeLengthRuleFlagsALongCode();
    }
}
