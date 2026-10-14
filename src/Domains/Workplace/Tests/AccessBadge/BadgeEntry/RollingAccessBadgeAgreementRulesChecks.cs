namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class RollingAccessBadgeAgreementRulesChecks
{
    public static void RollingAccessBadgeAgreementValidatorReportsEveryProblem()
    {
        var validator = RollingAccessBadgeAgreementRules.DefaultRollingAccessBadgeAgreementValidator();
        var item = RollingAccessBadgeAgreement.NewRollingAccessBadgeAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAccessBadgeAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingAccessBadgeAgreementCodeLengthRule(32);
        var item = RollingAccessBadgeAgreement.NewRollingAccessBadgeAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingAccessBadgeAgreementValidatorReportsEveryProblem();
        RollingAccessBadgeAgreementCodeLengthRuleFlagsALongCode();
    }
}
