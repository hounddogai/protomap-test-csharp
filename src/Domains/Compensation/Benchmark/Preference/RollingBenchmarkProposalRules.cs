namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling benchmark proposal must pass. Check returns its problems, or none.</summary>
public interface IRollingBenchmarkProposalRule
{
    List<string> Check(RollingBenchmarkProposal item);
}

public sealed record RollingBenchmarkProposalRequiredNameRule(string Message) : IRollingBenchmarkProposalRule
{
    public List<string> Check(RollingBenchmarkProposal item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkProposalCodeLengthRule(int MaxLength) : IRollingBenchmarkProposalRule
{
    public List<string> Check(RollingBenchmarkProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkProposalAllowedNameRule(List<string> Allowed) : IRollingBenchmarkProposalRule
{
    public List<string> Check(RollingBenchmarkProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling benchmark proposal.</summary>
public sealed record RollingBenchmarkProposalValidator(List<IRollingBenchmarkProposalRule> Rules)
{
    public List<string> Validate(RollingBenchmarkProposal item)
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

    public void RequireValid(RollingBenchmarkProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBenchmarkProposal> items)
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

/// <summary>Functions of rolling benchmark proposal rules.</summary>
public static class RollingBenchmarkProposalRules
{
    public static RollingBenchmarkProposalValidator DefaultRollingBenchmarkProposalValidator()
    {
        return new RollingBenchmarkProposalValidator(
            new List<IRollingBenchmarkProposalRule>
            {
                new RollingBenchmarkProposalRequiredNameRule("rolling benchmark proposal needs this field"),
                new RollingBenchmarkProposalCodeLengthRule(120),
                new RollingBenchmarkProposalAllowedNameRule(new List<string> { "lenient", "strict", "scheduled" }),
            });
    }
}
