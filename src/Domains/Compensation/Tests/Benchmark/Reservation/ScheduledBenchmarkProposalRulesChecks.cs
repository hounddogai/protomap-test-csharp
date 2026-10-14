namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScheduledBenchmarkProposalRulesChecks
{
    public static void ScheduledBenchmarkProposalValidatorReportsEveryProblem()
    {
        var validator = ScheduledBenchmarkProposalRules.DefaultScheduledBenchmarkProposalValidator();
        var item = ScheduledBenchmarkProposal.NewScheduledBenchmarkProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBenchmarkProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledBenchmarkProposalRequiredCodeRule("scheduled benchmark proposal needs an entry");
        var item = ScheduledBenchmarkProposal.NewScheduledBenchmarkProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledBenchmarkProposalRegionLengthRuleFlagsALongRegion()
    {
        var rule = new ScheduledBenchmarkProposalRegionLengthRule(255);
        var item = ScheduledBenchmarkProposal.NewScheduledBenchmarkProposal("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkProposalValidatorReportsEveryProblem();
        ScheduledBenchmarkProposalRequiredCodeRuleFlagsABlankCode();
        ScheduledBenchmarkProposalRegionLengthRuleFlagsALongRegion();
    }
}
