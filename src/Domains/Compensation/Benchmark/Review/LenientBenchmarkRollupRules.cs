namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient benchmark rollup must pass. Check returns its problems, or none.</summary>
public interface ILenientBenchmarkRollupRule
{
    List<string> Check(LenientBenchmarkRollup item);
}

public sealed record LenientBenchmarkRollupAllowedCodeRule(List<string> Allowed) : ILenientBenchmarkRollupRule
{
    public List<string> Check(LenientBenchmarkRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record LenientBenchmarkRollupAmountRangeRule(double Min, double Max) : ILenientBenchmarkRollupRule
{
    public List<string> Check(LenientBenchmarkRollup item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBenchmarkRollupExpiresAtRangeRule(int Min, int Max) : ILenientBenchmarkRollupRule
{
    public List<string> Check(LenientBenchmarkRollup item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBenchmarkRollupUpdatedAtRangeRule(int Min, int Max) : ILenientBenchmarkRollupRule
{
    public List<string> Check(LenientBenchmarkRollup item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBenchmarkRollupContactEmailLengthRule(int MaxLength) : ILenientBenchmarkRollupRule
{
    public List<string> Check(LenientBenchmarkRollup item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient benchmark rollup.</summary>
public sealed record LenientBenchmarkRollupValidator(List<ILenientBenchmarkRollupRule> Rules)
{
    public List<string> Validate(LenientBenchmarkRollup item)
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

    public void RequireValid(LenientBenchmarkRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBenchmarkRollup> items)
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

/// <summary>Functions of lenient benchmark rollup rules.</summary>
public static class LenientBenchmarkRollupRules
{
    public static LenientBenchmarkRollupValidator DefaultLenientBenchmarkRollupValidator()
    {
        return new LenientBenchmarkRollupValidator(
            new List<ILenientBenchmarkRollupRule>
            {
                new LenientBenchmarkRollupAllowedCodeRule(new List<string>()),
                new LenientBenchmarkRollupAmountRangeRule(6.0, 3289.6),
                new LenientBenchmarkRollupExpiresAtRangeRule(86400, 86400),
                new LenientBenchmarkRollupUpdatedAtRangeRule(16, 255),
                new LenientBenchmarkRollupContactEmailLengthRule(3600),
            });
    }
}
