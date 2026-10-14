namespace Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>A rule a strict recognition cycle must pass. Check returns its problems, or none.</summary>
public interface IStrictRecognitionCycleRule
{
    List<string> Check(StrictRecognitionCycle item);
}

public sealed record StrictRecognitionCycleRequiredCodeRule(string Message) : IStrictRecognitionCycleRule
{
    public List<string> Check(StrictRecognitionCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictRecognitionCycleCodePrefixRule(string Prefix) : IStrictRecognitionCycleRule
{
    public List<string> Check(StrictRecognitionCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRecognitionCycleRatioRangeRule(double Min, double Max) : IStrictRecognitionCycleRule
{
    public List<string> Check(StrictRecognitionCycle item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRecognitionCycleLimitRangeRule(int Min, int Max) : IStrictRecognitionCycleRule
{
    public List<string> Check(StrictRecognitionCycle item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictRecognitionCycleRequiredNameRule(string Message) : IStrictRecognitionCycleRule
{
    public List<string> Check(StrictRecognitionCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict recognition cycle.</summary>
public sealed record StrictRecognitionCycleValidator(List<IStrictRecognitionCycleRule> Rules)
{
    public List<string> Validate(StrictRecognitionCycle item)
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

    public void RequireValid(StrictRecognitionCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictRecognitionCycle> items)
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

/// <summary>Functions of strict recognition cycle rules.</summary>
public static class StrictRecognitionCycleRules
{
    public static StrictRecognitionCycleValidator DefaultStrictRecognitionCycleValidator()
    {
        return new StrictRecognitionCycleValidator(
            new List<IStrictRecognitionCycleRule>
            {
                new StrictRecognitionCycleRequiredCodeRule("strict recognition cycle needs this field"),
                new StrictRecognitionCycleCodePrefixRule("REF"),
                new StrictRecognitionCycleRatioRangeRule(7.7, 280.3),
                new StrictRecognitionCycleLimitRangeRule(16, 255),
                new StrictRecognitionCycleRequiredNameRule("strict recognition cycle needs an entry"),
            });
    }
}
