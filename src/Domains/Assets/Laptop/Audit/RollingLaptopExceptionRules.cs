namespace Acme.Workforce.Assets.Laptop.Audit;

/// <summary>A rule a rolling laptop exception must pass. Check returns its problems, or none.</summary>
public interface IRollingLaptopExceptionRule
{
    List<string> Check(RollingLaptopException item);
}

public sealed record RollingLaptopExceptionStartsAtRangeRule(int Min, int Max) : IRollingLaptopExceptionRule
{
    public List<string> Check(RollingLaptopException item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopExceptionCodePrefixRule(string Prefix) : IRollingLaptopExceptionRule
{
    public List<string> Check(RollingLaptopException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopExceptionCodeLengthRule(int MaxLength) : IRollingLaptopExceptionRule
{
    public List<string> Check(RollingLaptopException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling laptop exception.</summary>
public sealed record RollingLaptopExceptionValidator(List<IRollingLaptopExceptionRule> Rules)
{
    public List<string> Validate(RollingLaptopException item)
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

    public void RequireValid(RollingLaptopException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingLaptopException> items)
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

/// <summary>Functions of rolling laptop exception rules.</summary>
public static class RollingLaptopExceptionRules
{
    public static RollingLaptopExceptionValidator DefaultRollingLaptopExceptionValidator()
    {
        return new RollingLaptopExceptionValidator(
            new List<IRollingLaptopExceptionRule>
            {
                new RollingLaptopExceptionStartsAtRangeRule(80, 120),
                new RollingLaptopExceptionCodePrefixRule("Q"),
                new RollingLaptopExceptionCodeLengthRule(16),
            });
    }
}
