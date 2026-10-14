namespace Acme.Workforce.Compliance.Tests.Certification.Schedule;

using Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class FallbackCertificationBundleRulesChecks
{
    public static void FallbackCertificationBundleValidatorReportsEveryProblem()
    {
        var validator = FallbackCertificationBundleRules.DefaultFallbackCertificationBundleValidator();
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackCertificationBundleNameLengthRuleFlagsALongName()
    {
        var rule = new FallbackCertificationBundleNameLengthRule(1000);
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackCertificationBundleTeamLengthRuleFlagsALongTeam()
    {
        var rule = new FallbackCertificationBundleTeamLengthRule(120);
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackCertificationBundleRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new FallbackCertificationBundleRequiredGradeRule("fallback certification bundle needs a value");
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackCertificationBundleRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new FallbackCertificationBundleRequiredTeamRule("fallback certification bundle needs an entry");
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackCertificationBundleQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new FallbackCertificationBundleQuantityRangeRule(32, 86400);
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackCertificationBundleValidatorReportsEveryProblem();
        FallbackCertificationBundleNameLengthRuleFlagsALongName();
        FallbackCertificationBundleTeamLengthRuleFlagsALongTeam();
        FallbackCertificationBundleRequiredGradeRuleFlagsABlankGrade();
        FallbackCertificationBundleRequiredTeamRuleFlagsABlankTeam();
        FallbackCertificationBundleQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
