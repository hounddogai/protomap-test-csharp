namespace Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>A rule a laptop rollup must pass. Check returns its problems, or none.</summary>
public interface ILaptopRollupRule
{
    List<string> Check(LaptopRollup item);
}

public sealed record LaptopRollupCodePrefixRule(string Prefix) : ILaptopRollupRule
{
    public List<string> Check(LaptopRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopRollupTitleLengthRule(int MaxLength) : ILaptopRollupRule
{
    public List<string> Check(LaptopRollup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopRollupCodeLengthRule(int MaxLength) : ILaptopRollupRule
{
    public List<string> Check(LaptopRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop rollup.</summary>
public sealed record LaptopRollupValidator(List<ILaptopRollupRule> Rules)
{
    public List<string> Validate(LaptopRollup item)
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

    public void RequireValid(LaptopRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopRollup> items)
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

/// <summary>Functions of laptop rollup rules.</summary>
public static class LaptopRollupRules
{
    public static LaptopRollupValidator DefaultLaptopRollupValidator()
    {
        return new LaptopRollupValidator(
            new List<ILaptopRollupRule>
            {
                new LaptopRollupCodePrefixRule("REF"),
                new LaptopRollupTitleLengthRule(3600),
                new LaptopRollupCodeLengthRule(80),
            });
    }
}
