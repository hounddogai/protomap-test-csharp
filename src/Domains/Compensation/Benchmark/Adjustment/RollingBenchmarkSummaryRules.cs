namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>A rule a rolling benchmark summary must pass. Check returns its problems, or none.</summary>
public interface IRollingBenchmarkSummaryRule
{
    List<string> Check(RollingBenchmarkSummary item);
}

public sealed record RollingBenchmarkSummaryRetriesRangeRule(int Min, int Max) : IRollingBenchmarkSummaryRule
{
    public List<string> Check(RollingBenchmarkSummary item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkSummaryRegionLengthRule(int MaxLength) : IRollingBenchmarkSummaryRule
{
    public List<string> Check(RollingBenchmarkSummary item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkSummaryRequiredCodeRule(string Message) : IRollingBenchmarkSummaryRule
{
    public List<string> Check(RollingBenchmarkSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkSummaryCodeLengthRule(int MaxLength) : IRollingBenchmarkSummaryRule
{
    public List<string> Check(RollingBenchmarkSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkSummaryRequiredTitleRule(string Message) : IRollingBenchmarkSummaryRule
{
    public List<string> Check(RollingBenchmarkSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling benchmark summary.</summary>
public sealed record RollingBenchmarkSummaryValidator(List<IRollingBenchmarkSummaryRule> Rules)
{
    public List<string> Validate(RollingBenchmarkSummary item)
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

    public void RequireValid(RollingBenchmarkSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBenchmarkSummary> items)
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

/// <summary>Functions of rolling benchmark summary rules.</summary>
public static class RollingBenchmarkSummaryRules
{
    public static RollingBenchmarkSummaryValidator DefaultRollingBenchmarkSummaryValidator()
    {
        return new RollingBenchmarkSummaryValidator(
            new List<IRollingBenchmarkSummaryRule>
            {
                new RollingBenchmarkSummaryRetriesRangeRule(64, 64),
                new RollingBenchmarkSummaryRegionLengthRule(64),
                new RollingBenchmarkSummaryRequiredCodeRule("rolling benchmark summary needs this field"),
                new RollingBenchmarkSummaryCodeLengthRule(32),
                new RollingBenchmarkSummaryRequiredTitleRule("rolling benchmark summary needs a value"),
            });
    }
}
