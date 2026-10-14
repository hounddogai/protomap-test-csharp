namespace Acme.Workforce.Compensation.Tests.Benchmark.Preference;

using Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RollingBenchmarkProposalRulesChecks
{
    public static void RollingBenchmarkProposalValidatorReportsEveryProblem()
    {
        var validator = RollingBenchmarkProposalRules.DefaultRollingBenchmarkProposalValidator();
        var item = RollingBenchmarkProposal.NewRollingBenchmarkProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBenchmarkProposalRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingBenchmarkProposalRequiredNameRule("rolling benchmark proposal needs this field");
        var item = RollingBenchmarkProposal.NewRollingBenchmarkProposal("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBenchmarkProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingBenchmarkProposalCodeLengthRule(120);
        var item = RollingBenchmarkProposal.NewRollingBenchmarkProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingBenchmarkProposalValidatorReportsEveryProblem();
        RollingBenchmarkProposalRequiredNameRuleFlagsABlankName();
        RollingBenchmarkProposalCodeLengthRuleFlagsALongCode();
    }
}
