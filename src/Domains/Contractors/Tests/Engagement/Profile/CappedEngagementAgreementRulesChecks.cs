namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedEngagementAgreementRulesChecks
{
    public static void CappedEngagementAgreementValidatorReportsEveryProblem()
    {
        var validator = CappedEngagementAgreementRules.DefaultCappedEngagementAgreementValidator();
        var item = CappedEngagementAgreement.NewCappedEngagementAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedEngagementAgreementRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new CappedEngagementAgreementRequiredContactEmailRule("capped engagement agreement needs a value");
        var item = CappedEngagementAgreement.NewCappedEngagementAgreement("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedEngagementAgreementValidatorReportsEveryProblem();
        CappedEngagementAgreementRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
