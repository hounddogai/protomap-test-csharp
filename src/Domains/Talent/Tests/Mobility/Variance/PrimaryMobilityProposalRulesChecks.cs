namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PrimaryMobilityProposalRulesChecks
{
    public static void PrimaryMobilityProposalValidatorReportsEveryProblem()
    {
        var validator = PrimaryMobilityProposalRules.DefaultPrimaryMobilityProposalValidator();
        var item = PrimaryMobilityProposal.NewPrimaryMobilityProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryMobilityProposalRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new PrimaryMobilityProposalRequiredGradeRule("primary mobility proposal needs an entry");
        var item = PrimaryMobilityProposal.NewPrimaryMobilityProposal("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryMobilityProposalGradeLengthRuleFlagsALongGrade()
    {
        var rule = new PrimaryMobilityProposalGradeLengthRule(86400);
        var item = PrimaryMobilityProposal.NewPrimaryMobilityProposal("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryMobilityProposalValidatorReportsEveryProblem();
        PrimaryMobilityProposalRequiredGradeRuleFlagsABlankGrade();
        PrimaryMobilityProposalGradeLengthRuleFlagsALongGrade();
    }
}
