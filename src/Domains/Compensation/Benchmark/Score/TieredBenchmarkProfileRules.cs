namespace Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>A rule a tiered benchmark profile must pass. Check returns its problems, or none.</summary>
public interface ITieredBenchmarkProfileRule
{
    List<string> Check(TieredBenchmarkProfile item);
}

public sealed record TieredBenchmarkProfileCodeLengthRule(int MaxLength) : ITieredBenchmarkProfileRule
{
    public List<string> Check(TieredBenchmarkProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkProfileCreatedAtRangeRule(int Min, int Max) : ITieredBenchmarkProfileRule
{
    public List<string> Check(TieredBenchmarkProfile item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkProfileRequiredCodeRule(string Message) : ITieredBenchmarkProfileRule
{
    public List<string> Check(TieredBenchmarkProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkProfileRequiredOwnerIdRule(string Message) : ITieredBenchmarkProfileRule
{
    public List<string> Check(TieredBenchmarkProfile item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered benchmark profile.</summary>
public sealed record TieredBenchmarkProfileValidator(List<ITieredBenchmarkProfileRule> Rules)
{
    public List<string> Validate(TieredBenchmarkProfile item)
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

    public void RequireValid(TieredBenchmarkProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredBenchmarkProfile> items)
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

/// <summary>Functions of tiered benchmark profile rules.</summary>
public static class TieredBenchmarkProfileRules
{
    public static TieredBenchmarkProfileValidator DefaultTieredBenchmarkProfileValidator()
    {
        return new TieredBenchmarkProfileValidator(
            new List<ITieredBenchmarkProfileRule>
            {
                new TieredBenchmarkProfileCodeLengthRule(86400),
                new TieredBenchmarkProfileCreatedAtRangeRule(120, 86400),
                new TieredBenchmarkProfileRequiredCodeRule("tiered benchmark profile needs a value"),
                new TieredBenchmarkProfileRequiredOwnerIdRule("tiered benchmark profile needs a value"),
            });
    }
}
