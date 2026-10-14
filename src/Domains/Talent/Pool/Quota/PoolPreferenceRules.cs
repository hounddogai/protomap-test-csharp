namespace Acme.Workforce.Talent.Pool.Quota;

/// <summary>A rule a pool preference must pass. Check returns its problems, or none.</summary>
public interface IPoolPreferenceRule
{
    List<string> Check(PoolPreference item);
}

public sealed record PoolPreferenceCountryLengthRule(int MaxLength) : IPoolPreferenceRule
{
    public List<string> Check(PoolPreference item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolPreferenceCodePrefixRule(string Prefix) : IPoolPreferenceRule
{
    public List<string> Check(PoolPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolPreferenceRequiredCodeRule(string Message) : IPoolPreferenceRule
{
    public List<string> Check(PoolPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolPreferenceScoreRangeRule(double Min, double Max) : IPoolPreferenceRule
{
    public List<string> Check(PoolPreference item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolPreferenceLabelLengthRule(int MaxLength) : IPoolPreferenceRule
{
    public List<string> Check(PoolPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolPreferenceWindowDaysRangeRule(int Min, int Max) : IPoolPreferenceRule
{
    public List<string> Check(PoolPreference item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool preference.</summary>
public sealed record PoolPreferenceValidator(List<IPoolPreferenceRule> Rules)
{
    public List<string> Validate(PoolPreference item)
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

    public void RequireValid(PoolPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolPreference> items)
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

/// <summary>Functions of pool preference rules.</summary>
public static class PoolPreferenceRules
{
    public static PoolPreferenceValidator DefaultPoolPreferenceValidator()
    {
        return new PoolPreferenceValidator(
            new List<IPoolPreferenceRule>
            {
                new PoolPreferenceCountryLengthRule(255),
                new PoolPreferenceCodePrefixRule("X"),
                new PoolPreferenceRequiredCodeRule("pool preference needs this field"),
                new PoolPreferenceScoreRangeRule(4.8, 3457.6),
                new PoolPreferenceLabelLengthRule(32),
                new PoolPreferenceWindowDaysRangeRule(120, 3600),
            });
    }
}
