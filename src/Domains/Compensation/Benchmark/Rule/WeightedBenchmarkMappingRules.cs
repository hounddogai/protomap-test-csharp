namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted benchmark mapping must pass. Check returns its problems, or none.</summary>
public interface IWeightedBenchmarkMappingRule
{
    List<string> Check(WeightedBenchmarkMapping item);
}

public sealed record WeightedBenchmarkMappingTitleLengthRule(int MaxLength) : IWeightedBenchmarkMappingRule
{
    public List<string> Check(WeightedBenchmarkMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBenchmarkMappingCodePrefixRule(string Prefix) : IWeightedBenchmarkMappingRule
{
    public List<string> Check(WeightedBenchmarkMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBenchmarkMappingRequiredOwnerIdRule(string Message) : IWeightedBenchmarkMappingRule
{
    public List<string> Check(WeightedBenchmarkMapping item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBenchmarkMappingAllowedOwnerIdRule(List<string> Allowed) : IWeightedBenchmarkMappingRule
{
    public List<string> Check(WeightedBenchmarkMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted benchmark mapping.</summary>
public sealed record WeightedBenchmarkMappingValidator(List<IWeightedBenchmarkMappingRule> Rules)
{
    public List<string> Validate(WeightedBenchmarkMapping item)
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

    public void RequireValid(WeightedBenchmarkMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBenchmarkMapping> items)
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

/// <summary>Functions of weighted benchmark mapping rules.</summary>
public static class WeightedBenchmarkMappingRules
{
    public static WeightedBenchmarkMappingValidator DefaultWeightedBenchmarkMappingValidator()
    {
        return new WeightedBenchmarkMappingValidator(
            new List<IWeightedBenchmarkMappingRule>
            {
                new WeightedBenchmarkMappingTitleLengthRule(64),
                new WeightedBenchmarkMappingCodePrefixRule("Q"),
                new WeightedBenchmarkMappingRequiredOwnerIdRule("weighted benchmark mapping needs a value"),
                new WeightedBenchmarkMappingAllowedOwnerIdRule(new List<string> { "fallback", "scheduled" }),
            });
    }
}
