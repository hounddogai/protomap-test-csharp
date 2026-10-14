namespace Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>A rule a position profile must pass. Check returns its problems, or none.</summary>
public interface IPositionProfileRule
{
    List<string> Check(PositionProfile item);
}

public sealed record PositionProfileCodeLengthRule(int MaxLength) : IPositionProfileRule
{
    public List<string> Check(PositionProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionProfileCodePrefixRule(string Prefix) : IPositionProfileRule
{
    public List<string> Check(PositionProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionProfileRequiredSegmentRule(string Message) : IPositionProfileRule
{
    public List<string> Check(PositionProfile item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionProfileMaximumRangeRule(double Min, double Max) : IPositionProfileRule
{
    public List<string> Check(PositionProfile item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionProfileRequiredNameRule(string Message) : IPositionProfileRule
{
    public List<string> Check(PositionProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position profile.</summary>
public sealed record PositionProfileValidator(List<IPositionProfileRule> Rules)
{
    public List<string> Validate(PositionProfile item)
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

    public void RequireValid(PositionProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionProfile> items)
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

/// <summary>Functions of position profile rules.</summary>
public static class PositionProfileRules
{
    public static PositionProfileValidator DefaultPositionProfileValidator()
    {
        return new PositionProfileValidator(
            new List<IPositionProfileRule>
            {
                new PositionProfileCodeLengthRule(3600),
                new PositionProfileCodePrefixRule("REF"),
                new PositionProfileRequiredSegmentRule("position profile needs a value"),
                new PositionProfileMaximumRangeRule(5.8, 1495.5),
                new PositionProfileRequiredNameRule("position profile needs this field"),
            });
    }
}
