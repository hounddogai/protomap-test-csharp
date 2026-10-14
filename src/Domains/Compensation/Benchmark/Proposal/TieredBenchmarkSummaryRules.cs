namespace Acme.Workforce.Compensation.Benchmark.Proposal;

/// <summary>A rule a tiered benchmark summary must pass. Check returns its problems, or none.</summary>
public interface ITieredBenchmarkSummaryRule
{
    List<string> Check(TieredBenchmarkSummary item);
}

public sealed record TieredBenchmarkSummaryNameLengthRule(int MaxLength) : ITieredBenchmarkSummaryRule
{
    public List<string> Check(TieredBenchmarkSummary item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkSummaryRequiredNameRule(string Message) : ITieredBenchmarkSummaryRule
{
    public List<string> Check(TieredBenchmarkSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkSummaryRequiredEmployeeIdRule(string Message) : ITieredBenchmarkSummaryRule
{
    public List<string> Check(TieredBenchmarkSummary item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkSummaryRequiredPhoneRule(string Message) : ITieredBenchmarkSummaryRule
{
    public List<string> Check(TieredBenchmarkSummary item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered benchmark summary.</summary>
public sealed record TieredBenchmarkSummaryValidator(List<ITieredBenchmarkSummaryRule> Rules)
{
    public List<string> Validate(TieredBenchmarkSummary item)
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

    public void RequireValid(TieredBenchmarkSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredBenchmarkSummary> items)
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

/// <summary>Functions of tiered benchmark summary rules.</summary>
public static class TieredBenchmarkSummaryRules
{
    public static TieredBenchmarkSummaryValidator DefaultTieredBenchmarkSummaryValidator()
    {
        return new TieredBenchmarkSummaryValidator(
            new List<ITieredBenchmarkSummaryRule>
            {
                new TieredBenchmarkSummaryNameLengthRule(16),
                new TieredBenchmarkSummaryRequiredNameRule("tiered benchmark summary needs a value"),
                new TieredBenchmarkSummaryRequiredEmployeeIdRule("tiered benchmark summary needs a value"),
                new TieredBenchmarkSummaryRequiredPhoneRule("tiered benchmark summary needs a value"),
            });
    }
}
