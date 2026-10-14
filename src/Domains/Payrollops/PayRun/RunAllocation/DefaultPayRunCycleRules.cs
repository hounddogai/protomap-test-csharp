namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>A rule a default pay run cycle must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayRunCycleRule
{
    List<string> Check(DefaultPayRunCycle item);
}

public sealed record DefaultPayRunCycleCodeLengthRule(int MaxLength) : IDefaultPayRunCycleRule
{
    public List<string> Check(DefaultPayRunCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayRunCycleCodePrefixRule(string Prefix) : IDefaultPayRunCycleRule
{
    public List<string> Check(DefaultPayRunCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayRunCycleRequiredJobCodeRule(string Message) : IDefaultPayRunCycleRule
{
    public List<string> Check(DefaultPayRunCycle item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayRunCycleRequiredCodeRule(string Message) : IDefaultPayRunCycleRule
{
    public List<string> Check(DefaultPayRunCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayRunCycleRateRangeRule(double Min, double Max) : IDefaultPayRunCycleRule
{
    public List<string> Check(DefaultPayRunCycle item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default pay run cycle.</summary>
public sealed record DefaultPayRunCycleValidator(List<IDefaultPayRunCycleRule> Rules)
{
    public List<string> Validate(DefaultPayRunCycle item)
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

    public void RequireValid(DefaultPayRunCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayRunCycle> items)
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

/// <summary>Functions of default pay run cycle rules.</summary>
public static class DefaultPayRunCycleRules
{
    public static DefaultPayRunCycleValidator DefaultDefaultPayRunCycleValidator()
    {
        return new DefaultPayRunCycleValidator(
            new List<IDefaultPayRunCycleRule>
            {
                new DefaultPayRunCycleCodeLengthRule(86400),
                new DefaultPayRunCycleCodePrefixRule("ACM"),
                new DefaultPayRunCycleRequiredJobCodeRule("default pay run cycle needs this field"),
                new DefaultPayRunCycleRequiredCodeRule("default pay run cycle needs this field"),
                new DefaultPayRunCycleRateRangeRule(0.5, 230.5),
            });
    }
}
