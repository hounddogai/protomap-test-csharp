namespace Acme.Workforce.Orgdesign.Headcount.Draft;

/// <summary>A rule a default headcount target must pass. Check returns its problems, or none.</summary>
public interface IDefaultHeadcountTargetRule
{
    List<string> Check(DefaultHeadcountTarget item);
}

public sealed record DefaultHeadcountTargetWeightRangeRule(double Min, double Max) : IDefaultHeadcountTargetRule
{
    public List<string> Check(DefaultHeadcountTarget item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultHeadcountTargetRequiredNameRule(string Message) : IDefaultHeadcountTargetRule
{
    public List<string> Check(DefaultHeadcountTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountTargetDurationMinutesRangeRule(int Min, int Max) : IDefaultHeadcountTargetRule
{
    public List<string> Check(DefaultHeadcountTarget item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountTargetCodeLengthRule(int MaxLength) : IDefaultHeadcountTargetRule
{
    public List<string> Check(DefaultHeadcountTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountTargetRequiredCodeRule(string Message) : IDefaultHeadcountTargetRule
{
    public List<string> Check(DefaultHeadcountTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountTargetThresholdRangeRule(double Min, double Max) : IDefaultHeadcountTargetRule
{
    public List<string> Check(DefaultHeadcountTarget item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default headcount target.</summary>
public sealed record DefaultHeadcountTargetValidator(List<IDefaultHeadcountTargetRule> Rules)
{
    public List<string> Validate(DefaultHeadcountTarget item)
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

    public void RequireValid(DefaultHeadcountTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultHeadcountTarget> items)
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

/// <summary>Functions of default headcount target rules.</summary>
public static class DefaultHeadcountTargetRules
{
    public static DefaultHeadcountTargetValidator DefaultDefaultHeadcountTargetValidator()
    {
        return new DefaultHeadcountTargetValidator(
            new List<IDefaultHeadcountTargetRule>
            {
                new DefaultHeadcountTargetWeightRangeRule(0.1, 3563.5),
                new DefaultHeadcountTargetRequiredNameRule("default headcount target needs an entry"),
                new DefaultHeadcountTargetDurationMinutesRangeRule(255, 1000),
                new DefaultHeadcountTargetCodeLengthRule(120),
                new DefaultHeadcountTargetRequiredCodeRule("default headcount target needs a value"),
                new DefaultHeadcountTargetThresholdRangeRule(4.1, 1226.4),
            });
    }
}
