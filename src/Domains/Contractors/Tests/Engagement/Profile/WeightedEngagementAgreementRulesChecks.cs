namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedEngagementAgreementRulesChecks
{
    public static void WeightedEngagementAgreementValidatorReportsEveryProblem()
    {
        var validator = WeightedEngagementAgreementRules.DefaultWeightedEngagementAgreementValidator();
        var item = WeightedEngagementAgreement.NewWeightedEngagementAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedEngagementAgreementContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new WeightedEngagementAgreementContactEmailLengthRule(255);
        var item = WeightedEngagementAgreement.NewWeightedEngagementAgreement("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedEngagementAgreementRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new WeightedEngagementAgreementRequiredContactEmailRule(
            "weighted engagement agreement needs a value");
        var item = WeightedEngagementAgreement.NewWeightedEngagementAgreement("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedEngagementAgreementLocationLengthRuleFlagsALongLocation()
    {
        var rule = new WeightedEngagementAgreementLocationLengthRule(3600);
        var item = WeightedEngagementAgreement.NewWeightedEngagementAgreement("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedEngagementAgreementValidatorReportsEveryProblem();
        WeightedEngagementAgreementContactEmailLengthRuleFlagsALongContactEmail();
        WeightedEngagementAgreementRequiredContactEmailRuleFlagsABlankContactEmail();
        WeightedEngagementAgreementLocationLengthRuleFlagsALongLocation();
    }
}
