namespace Acme.Workforce.Talent.Mobility.Target;

/// <summary>A rule a default mobility target must pass. Check returns its problems, or none.</summary>
public interface IDefaultMobilityTargetRule
{
    List<string> Check(DefaultMobilityTarget item);
}

public sealed record DefaultMobilityTargetRequiredCodeRule(string Message) : IDefaultMobilityTargetRule
{
    public List<string> Check(DefaultMobilityTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultMobilityTargetWindowDaysRangeRule(int Min, int Max) : IDefaultMobilityTargetRule
{
    public List<string> Check(DefaultMobilityTarget item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultMobilityTargetCodeLengthRule(int MaxLength) : IDefaultMobilityTargetRule
{
    public List<string> Check(DefaultMobilityTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultMobilityTargetRequiredTitleRule(string Message) : IDefaultMobilityTargetRule
{
    public List<string> Check(DefaultMobilityTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultMobilityTargetRequiredDepartmentRule(string Message) : IDefaultMobilityTargetRule
{
    public List<string> Check(DefaultMobilityTarget item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultMobilityTargetRetriesRangeRule(int Min, int Max) : IDefaultMobilityTargetRule
{
    public List<string> Check(DefaultMobilityTarget item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default mobility target.</summary>
public sealed record DefaultMobilityTargetValidator(List<IDefaultMobilityTargetRule> Rules)
{
    public List<string> Validate(DefaultMobilityTarget item)
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

    public void RequireValid(DefaultMobilityTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultMobilityTarget> items)
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

/// <summary>Functions of default mobility target rules.</summary>
public static class DefaultMobilityTargetRules
{
    public static DefaultMobilityTargetValidator DefaultDefaultMobilityTargetValidator()
    {
        return new DefaultMobilityTargetValidator(
            new List<IDefaultMobilityTargetRule>
            {
                new DefaultMobilityTargetRequiredCodeRule("default mobility target needs an entry"),
                new DefaultMobilityTargetWindowDaysRangeRule(16, 86400),
                new DefaultMobilityTargetCodeLengthRule(32),
                new DefaultMobilityTargetRequiredTitleRule("default mobility target needs a value"),
                new DefaultMobilityTargetRequiredDepartmentRule("default mobility target needs this field"),
                new DefaultMobilityTargetRetriesRangeRule(32, 1000),
            });
    }
}
