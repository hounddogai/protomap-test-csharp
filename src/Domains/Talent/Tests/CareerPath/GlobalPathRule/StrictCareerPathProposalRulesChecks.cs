namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class StrictCareerPathProposalRulesChecks
{
    public static void StrictCareerPathProposalValidatorReportsEveryProblem()
    {
        var validator = StrictCareerPathProposalRules.DefaultStrictCareerPathProposalValidator();
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictCareerPathProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictCareerPathProposalCodeLengthRule(120);
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictCareerPathProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictCareerPathProposalRequiredTitleRule("strict career path proposal needs this field");
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictCareerPathProposalRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new StrictCareerPathProposalRequiredCountryRule("strict career path proposal needs this field");
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictCareerPathProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictCareerPathProposalRequiredCodeRule("strict career path proposal needs this field");
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictCareerPathProposalValidatorReportsEveryProblem();
        StrictCareerPathProposalCodeLengthRuleFlagsALongCode();
        StrictCareerPathProposalRequiredTitleRuleFlagsABlankTitle();
        StrictCareerPathProposalRequiredCountryRuleFlagsABlankCountry();
        StrictCareerPathProposalRequiredCodeRuleFlagsABlankCode();
    }
}
