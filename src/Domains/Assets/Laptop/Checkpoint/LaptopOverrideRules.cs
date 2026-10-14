namespace Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>A rule a laptop override must pass. Check returns its problems, or none.</summary>
public interface ILaptopOverrideRule
{
    List<string> Check(LaptopOverride item);
}

public sealed record LaptopOverrideCodePrefixRule(string Prefix) : ILaptopOverrideRule
{
    public List<string> Check(LaptopOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopOverrideVersionRangeRule(int Min, int Max) : ILaptopOverrideRule
{
    public List<string> Check(LaptopOverride item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopOverrideEmployeeIdLengthRule(int MaxLength) : ILaptopOverrideRule
{
    public List<string> Check(LaptopOverride item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopOverrideRequiredCodeRule(string Message) : ILaptopOverrideRule
{
    public List<string> Check(LaptopOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop override.</summary>
public sealed record LaptopOverrideValidator(List<ILaptopOverrideRule> Rules)
{
    public List<string> Validate(LaptopOverride item)
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

    public void RequireValid(LaptopOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopOverride> items)
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

/// <summary>Functions of laptop override rules.</summary>
public static class LaptopOverrideRules
{
    public static LaptopOverrideValidator DefaultLaptopOverrideValidator()
    {
        return new LaptopOverrideValidator(
            new List<ILaptopOverrideRule>
            {
                new LaptopOverrideCodePrefixRule("X"),
                new LaptopOverrideVersionRangeRule(16, 1000),
                new LaptopOverrideEmployeeIdLengthRule(1000),
                new LaptopOverrideRequiredCodeRule("laptop override needs this field"),
            });
    }
}
