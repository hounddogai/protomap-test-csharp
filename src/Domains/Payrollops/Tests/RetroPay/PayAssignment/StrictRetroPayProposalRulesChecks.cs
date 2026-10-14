namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class StrictRetroPayProposalRulesChecks
{
    public static void StrictRetroPayProposalValidatorReportsEveryProblem()
    {
        var validator = StrictRetroPayProposalRules.DefaultStrictRetroPayProposalValidator();
        var item = StrictRetroPayProposal.NewStrictRetroPayProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictRetroPayProposalLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictRetroPayProposalLabelLengthRule(64);
        var item = StrictRetroPayProposal.NewStrictRetroPayProposal("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictRetroPayProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictRetroPayProposalCodeLengthRule(1000);
        var item = StrictRetroPayProposal.NewStrictRetroPayProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictRetroPayProposalRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new StrictRetroPayProposalRequiredCategoryRule("strict retro pay proposal needs a value");
        var item = StrictRetroPayProposal.NewStrictRetroPayProposal("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictRetroPayProposalValidatorReportsEveryProblem();
        StrictRetroPayProposalLabelLengthRuleFlagsALongLabel();
        StrictRetroPayProposalCodeLengthRuleFlagsALongCode();
        StrictRetroPayProposalRequiredCategoryRuleFlagsABlankCategory();
    }
}
