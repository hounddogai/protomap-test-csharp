namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>A rule a fallback pay run cycle must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayRunCycleRule
{
    List<string> Check(FallbackPayRunCycle item);
}

public sealed record FallbackPayRunCycleRequiredCodeRule(string Message) : IFallbackPayRunCycleRule
{
    public List<string> Check(FallbackPayRunCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayRunCycleCodePrefixRule(string Prefix) : IFallbackPayRunCycleRule
{
    public List<string> Check(FallbackPayRunCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayRunCycleCurrencyLengthRule(int MaxLength) : IFallbackPayRunCycleRule
{
    public List<string> Check(FallbackPayRunCycle item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayRunCycleCreatedAtRangeRule(int Min, int Max) : IFallbackPayRunCycleRule
{
    public List<string> Check(FallbackPayRunCycle item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback pay run cycle.</summary>
public sealed record FallbackPayRunCycleValidator(List<IFallbackPayRunCycleRule> Rules)
{
    public List<string> Validate(FallbackPayRunCycle item)
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

    public void RequireValid(FallbackPayRunCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayRunCycle> items)
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

/// <summary>Functions of fallback pay run cycle rules.</summary>
public static class FallbackPayRunCycleRules
{
    public static FallbackPayRunCycleValidator DefaultFallbackPayRunCycleValidator()
    {
        return new FallbackPayRunCycleValidator(
            new List<IFallbackPayRunCycleRule>
            {
                new FallbackPayRunCycleRequiredCodeRule("fallback pay run cycle needs a value"),
                new FallbackPayRunCycleCodePrefixRule("OPS"),
                new FallbackPayRunCycleCurrencyLengthRule(80),
                new FallbackPayRunCycleCreatedAtRangeRule(80, 120),
            });
    }
}
