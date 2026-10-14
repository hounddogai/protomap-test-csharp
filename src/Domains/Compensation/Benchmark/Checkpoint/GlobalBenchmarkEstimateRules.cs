namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

/// <summary>A rule a global benchmark estimate must pass. Check returns its problems, or none.</summary>
public interface IGlobalBenchmarkEstimateRule
{
    List<string> Check(GlobalBenchmarkEstimate item);
}

public sealed record GlobalBenchmarkEstimateContactEmailLengthRule(int MaxLength) : IGlobalBenchmarkEstimateRule
{
    public List<string> Check(GlobalBenchmarkEstimate item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkEstimateCodePrefixRule(string Prefix) : IGlobalBenchmarkEstimateRule
{
    public List<string> Check(GlobalBenchmarkEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkEstimateRequiredContactEmailRule(string Message) : IGlobalBenchmarkEstimateRule
{
    public List<string> Check(GlobalBenchmarkEstimate item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global benchmark estimate.</summary>
public sealed record GlobalBenchmarkEstimateValidator(List<IGlobalBenchmarkEstimateRule> Rules)
{
    public List<string> Validate(GlobalBenchmarkEstimate item)
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

    public void RequireValid(GlobalBenchmarkEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalBenchmarkEstimate> items)
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

/// <summary>Functions of global benchmark estimate rules.</summary>
public static class GlobalBenchmarkEstimateRules
{
    public static GlobalBenchmarkEstimateValidator DefaultGlobalBenchmarkEstimateValidator()
    {
        return new GlobalBenchmarkEstimateValidator(
            new List<IGlobalBenchmarkEstimateRule>
            {
                new GlobalBenchmarkEstimateContactEmailLengthRule(16),
                new GlobalBenchmarkEstimateCodePrefixRule("Q"),
                new GlobalBenchmarkEstimateRequiredContactEmailRule("global benchmark estimate needs this field"),
            });
    }
}
