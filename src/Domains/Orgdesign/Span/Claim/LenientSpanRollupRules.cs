namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient span rollup must pass. Check returns its problems, or none.</summary>
public interface ILenientSpanRollupRule
{
    List<string> Check(LenientSpanRollup item);
}

public sealed record LenientSpanRollupEmployeeIdLengthRule(int MaxLength) : ILenientSpanRollupRule
{
    public List<string> Check(LenientSpanRollup item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSpanRollupLeaveDaysRangeRule(int Min, int Max) : ILenientSpanRollupRule
{
    public List<string> Check(LenientSpanRollup item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSpanRollupAttemptsRangeRule(int Min, int Max) : ILenientSpanRollupRule
{
    public List<string> Check(LenientSpanRollup item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientSpanRollupCodePrefixRule(string Prefix) : ILenientSpanRollupRule
{
    public List<string> Check(LenientSpanRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSpanRollupAllowedNameRule(List<string> Allowed) : ILenientSpanRollupRule
{
    public List<string> Check(LenientSpanRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient span rollup.</summary>
public sealed record LenientSpanRollupValidator(List<ILenientSpanRollupRule> Rules)
{
    public List<string> Validate(LenientSpanRollup item)
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

    public void RequireValid(LenientSpanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSpanRollup> items)
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

/// <summary>Functions of lenient span rollup rules.</summary>
public static class LenientSpanRollupRules
{
    public static LenientSpanRollupValidator DefaultLenientSpanRollupValidator()
    {
        return new LenientSpanRollupValidator(
            new List<ILenientSpanRollupRule>
            {
                new LenientSpanRollupEmployeeIdLengthRule(32),
                new LenientSpanRollupLeaveDaysRangeRule(16, 32),
                new LenientSpanRollupAttemptsRangeRule(255, 1000),
                new LenientSpanRollupCodePrefixRule("REF"),
                new LenientSpanRollupAllowedNameRule(new List<string> { "capped", "tiered", "scheduled" }),
            });
    }
}
