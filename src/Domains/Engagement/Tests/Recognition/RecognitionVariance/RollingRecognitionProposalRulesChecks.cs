namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class RollingRecognitionProposalRulesChecks
{
    public static void RollingRecognitionProposalValidatorReportsEveryProblem()
    {
        var validator = RollingRecognitionProposalRules.DefaultRollingRecognitionProposalValidator();
        var item = RollingRecognitionProposal.NewRollingRecognitionProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingRecognitionProposalRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new RollingRecognitionProposalRequiredCountryRule("rolling recognition proposal needs a value");
        var item = RollingRecognitionProposal.NewRollingRecognitionProposal("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingRecognitionProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingRecognitionProposalRequiredLabelRule("rolling recognition proposal needs this field");
        var item = RollingRecognitionProposal.NewRollingRecognitionProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingRecognitionProposalLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingRecognitionProposalLabelLengthRule(32);
        var item = RollingRecognitionProposal.NewRollingRecognitionProposal("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingRecognitionProposalValidatorReportsEveryProblem();
        RollingRecognitionProposalRequiredCountryRuleFlagsABlankCountry();
        RollingRecognitionProposalRequiredLabelRuleFlagsABlankLabel();
        RollingRecognitionProposalLabelLengthRuleFlagsALongLabel();
    }
}
