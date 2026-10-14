namespace Acme.Workforce.Orgdesign.Position.Template;

/// <summary>A rule a position cycle must pass. Check returns its problems, or none.</summary>
public interface IPositionCycleRule
{
    List<string> Check(PositionCycle item);
}

public sealed record PositionCycleTitleLengthRule(int MaxLength) : IPositionCycleRule
{
    public List<string> Check(PositionCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionCycleThresholdRangeRule(double Min, double Max) : IPositionCycleRule
{
    public List<string> Check(PositionCycle item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionCycleCodePrefixRule(string Prefix) : IPositionCycleRule
{
    public List<string> Check(PositionCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionCycleRequiredTitleRule(string Message) : IPositionCycleRule
{
    public List<string> Check(PositionCycle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position cycle.</summary>
public sealed record PositionCycleValidator(List<IPositionCycleRule> Rules)
{
    public List<string> Validate(PositionCycle item)
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

    public void RequireValid(PositionCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionCycle> items)
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

/// <summary>Functions of position cycle rules.</summary>
public static class PositionCycleRules
{
    public static PositionCycleValidator DefaultPositionCycleValidator()
    {
        return new PositionCycleValidator(
            new List<IPositionCycleRule>
            {
                new PositionCycleTitleLengthRule(64),
                new PositionCycleThresholdRangeRule(5.1, 418.4),
                new PositionCycleCodePrefixRule("OPS"),
                new PositionCycleRequiredTitleRule("position cycle needs a value"),
            });
    }
}
