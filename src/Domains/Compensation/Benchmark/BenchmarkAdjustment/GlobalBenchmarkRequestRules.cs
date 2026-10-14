namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>A rule a global benchmark request must pass. Check returns its problems, or none.</summary>
public interface IGlobalBenchmarkRequestRule
{
    List<string> Check(GlobalBenchmarkRequest item);
}

public sealed record GlobalBenchmarkRequestRequiredManagerIdRule(string Message) : IGlobalBenchmarkRequestRule
{
    public List<string> Check(GlobalBenchmarkRequest item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkRequestCodePrefixRule(string Prefix) : IGlobalBenchmarkRequestRule
{
    public List<string> Check(GlobalBenchmarkRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkRequestManagerIdLengthRule(int MaxLength) : IGlobalBenchmarkRequestRule
{
    public List<string> Check(GlobalBenchmarkRequest item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkRequestRequiredLabelRule(string Message) : IGlobalBenchmarkRequestRule
{
    public List<string> Check(GlobalBenchmarkRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkRequestRequiredCodeRule(string Message) : IGlobalBenchmarkRequestRule
{
    public List<string> Check(GlobalBenchmarkRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global benchmark request.</summary>
public sealed record GlobalBenchmarkRequestValidator(List<IGlobalBenchmarkRequestRule> Rules)
{
    public List<string> Validate(GlobalBenchmarkRequest item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(GlobalBenchmarkRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalBenchmarkRequest> items)
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

/// <summary>Functions of global benchmark request rules.</summary>
public static class GlobalBenchmarkRequestRules
{
    public static GlobalBenchmarkRequestValidator DefaultGlobalBenchmarkRequestValidator()
    {
        return new GlobalBenchmarkRequestValidator(
            new List<IGlobalBenchmarkRequestRule>
            {
                new GlobalBenchmarkRequestRequiredManagerIdRule("global benchmark request needs an entry"),
                new GlobalBenchmarkRequestCodePrefixRule("ACM"),
                new GlobalBenchmarkRequestManagerIdLengthRule(80),
                new GlobalBenchmarkRequestRequiredLabelRule("global benchmark request needs a value"),
                new GlobalBenchmarkRequestRequiredCodeRule("global benchmark request needs an entry"),
            });
    }
}
