namespace Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>A rule a default rate cycle must pass. Check returns its problems, or none.</summary>
public interface IDefaultRateCycleRule
{
    List<string> Check(DefaultRateCycle item);
}

public sealed record DefaultRateCycleRequiredCodeRule(string Message) : IDefaultRateCycleRule
{
    public List<string> Check(DefaultRateCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRateCycleVersionRangeRule(int Min, int Max) : IDefaultRateCycleRule
{
    public List<string> Check(DefaultRateCycle item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRateCycleRequiredRegionRule(string Message) : IDefaultRateCycleRule
{
    public List<string> Check(DefaultRateCycle item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRateCycleLocationLengthRule(int MaxLength) : IDefaultRateCycleRule
{
    public List<string> Check(DefaultRateCycle item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRateCycleCodeLengthRule(int MaxLength) : IDefaultRateCycleRule
{
    public List<string> Check(DefaultRateCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRateCycleAttemptsRangeRule(int Min, int Max) : IDefaultRateCycleRule
{
    public List<string> Check(DefaultRateCycle item)
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

/// <summary>Runs every rule of a default rate cycle.</summary>
public sealed record DefaultRateCycleValidatorExtra(List<IDefaultRateCycleRule> Rules)
{
    public List<string> Validate(DefaultRateCycle item)
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

    public void RequireValid(DefaultRateCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultRateCycle> items)
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

/// <summary>Functions of default rate cycle rules.</summary>
public static class DefaultRateCycleRules
{
    public static DefaultRateCycleValidatorExtra DefaultDefaultRateCycleValidator()
    {
        return new DefaultRateCycleValidatorExtra(
            new List<IDefaultRateCycleRule>
            {
                new DefaultRateCycleRequiredCodeRule("default rate cycle needs a value"),
                new DefaultRateCycleVersionRangeRule(16, 32),
                new DefaultRateCycleRequiredRegionRule("default rate cycle needs a value"),
                new DefaultRateCycleLocationLengthRule(80),
                new DefaultRateCycleCodeLengthRule(16),
                new DefaultRateCycleAttemptsRangeRule(64, 255),
            });
    }
}
