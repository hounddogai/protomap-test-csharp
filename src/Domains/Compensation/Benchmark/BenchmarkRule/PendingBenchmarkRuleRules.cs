namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

/// <summary>A rule a pending benchmark rule must pass. Check returns its problems, or none.</summary>
public interface IPendingBenchmarkRuleRule
{
    List<string> Check(PendingBenchmarkRule item);
}

public sealed record PendingBenchmarkRuleRequiredCodeRule(string Message) : IPendingBenchmarkRuleRule
{
    public List<string> Check(PendingBenchmarkRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingBenchmarkRuleNameLengthRule(int MaxLength) : IPendingBenchmarkRuleRule
{
    public List<string> Check(PendingBenchmarkRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBenchmarkRuleCodePrefixRule(string Prefix) : IPendingBenchmarkRuleRule
{
    public List<string> Check(PendingBenchmarkRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBenchmarkRuleCodeLengthRule(int MaxLength) : IPendingBenchmarkRuleRule
{
    public List<string> Check(PendingBenchmarkRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBenchmarkRuleGraceDaysRangeRule(int Min, int Max) : IPendingBenchmarkRuleRule
{
    public List<string> Check(PendingBenchmarkRule item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingBenchmarkRuleFteRangeRule(double Min, double Max) : IPendingBenchmarkRuleRule
{
    public List<string> Check(PendingBenchmarkRule item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending benchmark rule.</summary>
public sealed record PendingBenchmarkRuleValidator(List<IPendingBenchmarkRuleRule> Rules)
{
    public List<string> Validate(PendingBenchmarkRule item)
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

    public void RequireValid(PendingBenchmarkRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingBenchmarkRule> items)
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

/// <summary>Functions of pending benchmark rule rules.</summary>
public static class PendingBenchmarkRuleRules
{
    public static PendingBenchmarkRuleValidator DefaultPendingBenchmarkRuleValidator()
    {
        return new PendingBenchmarkRuleValidator(
            new List<IPendingBenchmarkRuleRule>
            {
                new PendingBenchmarkRuleRequiredCodeRule("pending benchmark rule needs an entry"),
                new PendingBenchmarkRuleNameLengthRule(120),
                new PendingBenchmarkRuleCodePrefixRule("X"),
                new PendingBenchmarkRuleCodeLengthRule(1000),
                new PendingBenchmarkRuleGraceDaysRangeRule(80, 255),
                new PendingBenchmarkRuleFteRangeRule(4.8, 1757.4),
            });
    }
}
