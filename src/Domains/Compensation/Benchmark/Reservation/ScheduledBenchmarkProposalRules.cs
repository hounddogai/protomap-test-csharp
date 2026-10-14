namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled benchmark proposal must pass. Check returns its problems, or none.</summary>
public interface IScheduledBenchmarkProposalRule
{
    List<string> Check(ScheduledBenchmarkProposal item);
}

public sealed record ScheduledBenchmarkProposalRequiredCodeRule(string Message) : IScheduledBenchmarkProposalRule
{
    public List<string> Check(ScheduledBenchmarkProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkProposalAllowedTitleRule(List<string> Allowed) : IScheduledBenchmarkProposalRule
{
    public List<string> Check(ScheduledBenchmarkProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkProposalRegionLengthRule(int MaxLength) : IScheduledBenchmarkProposalRule
{
    public List<string> Check(ScheduledBenchmarkProposal item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkProposalCodePrefixRule(string Prefix) : IScheduledBenchmarkProposalRule
{
    public List<string> Check(ScheduledBenchmarkProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled benchmark proposal.</summary>
public sealed record ScheduledBenchmarkProposalValidator(List<IScheduledBenchmarkProposalRule> Rules)
{
    public List<string> Validate(ScheduledBenchmarkProposal item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(ScheduledBenchmarkProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBenchmarkProposal> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of scheduled benchmark proposal rules.</summary>
public static class ScheduledBenchmarkProposalRules
{
    public static ScheduledBenchmarkProposalValidator DefaultScheduledBenchmarkProposalValidator()
    {
        return new ScheduledBenchmarkProposalValidator(
            new List<IScheduledBenchmarkProposalRule>
            {
                new ScheduledBenchmarkProposalRequiredCodeRule("scheduled benchmark proposal needs an entry"),
                new ScheduledBenchmarkProposalAllowedTitleRule(new List<string>()),
                new ScheduledBenchmarkProposalRegionLengthRule(1000),
                new ScheduledBenchmarkProposalCodePrefixRule("REF"),
            });
    }
}
