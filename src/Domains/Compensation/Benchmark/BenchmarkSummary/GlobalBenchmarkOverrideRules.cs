namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>A rule a global benchmark override must pass. Check returns its problems, or none.</summary>
public interface IGlobalBenchmarkOverrideRule
{
    List<string> Check(GlobalBenchmarkOverride item);
}

public sealed record GlobalBenchmarkOverrideRequiredCodeRule(string Message) : IGlobalBenchmarkOverrideRule
{
    public List<string> Check(GlobalBenchmarkOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkOverrideCategoryLengthRule(int MaxLength) : IGlobalBenchmarkOverrideRule
{
    public List<string> Check(GlobalBenchmarkOverride item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkOverrideAttemptsRangeRule(int Min, int Max) : IGlobalBenchmarkOverrideRule
{
    public List<string> Check(GlobalBenchmarkOverride item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkOverrideRequiredLabelRule(string Message) : IGlobalBenchmarkOverrideRule
{
    public List<string> Check(GlobalBenchmarkOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkOverrideCostCenterLengthRule(int MaxLength) : IGlobalBenchmarkOverrideRule
{
    public List<string> Check(GlobalBenchmarkOverride item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global benchmark override.</summary>
public sealed record GlobalBenchmarkOverrideValidator(List<IGlobalBenchmarkOverrideRule> Rules)
{
    public List<string> Validate(GlobalBenchmarkOverride item)
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

    public void RequireValid(GlobalBenchmarkOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalBenchmarkOverride> items)
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

/// <summary>Functions of global benchmark override rules.</summary>
public static class GlobalBenchmarkOverrideRules
{
    public static GlobalBenchmarkOverrideValidator DefaultGlobalBenchmarkOverrideValidator()
    {
        return new GlobalBenchmarkOverrideValidator(
            new List<IGlobalBenchmarkOverrideRule>
            {
                new GlobalBenchmarkOverrideRequiredCodeRule("global benchmark override needs an entry"),
                new GlobalBenchmarkOverrideCategoryLengthRule(64),
                new GlobalBenchmarkOverrideAttemptsRangeRule(64, 3600),
                new GlobalBenchmarkOverrideRequiredLabelRule("global benchmark override needs a value"),
                new GlobalBenchmarkOverrideCostCenterLengthRule(3600),
            });
    }
}
