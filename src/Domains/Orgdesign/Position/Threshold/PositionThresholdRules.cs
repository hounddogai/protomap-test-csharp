namespace Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>A rule a position threshold must pass. Check returns its problems, or none.</summary>
public interface IPositionThresholdRule
{
    List<string> Check(PositionThreshold item);
}

public sealed record PositionThresholdAttemptsRangeRule(int Min, int Max) : IPositionThresholdRule
{
    public List<string> Check(PositionThreshold item)
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

public sealed record PositionThresholdRequiredLabelRule(string Message) : IPositionThresholdRule
{
    public List<string> Check(PositionThreshold item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionThresholdRatingRangeRule(double Min, double Max) : IPositionThresholdRule
{
    public List<string> Check(PositionThreshold item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionThresholdRequiredExternalRefRule(string Message) : IPositionThresholdRule
{
    public List<string> Check(PositionThreshold item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionThresholdCodeLengthRule(int MaxLength) : IPositionThresholdRule
{
    public List<string> Check(PositionThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionThresholdDiscountRangeRule(double Min, double Max) : IPositionThresholdRule
{
    public List<string> Check(PositionThreshold item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position threshold.</summary>
public sealed record PositionThresholdValidator(List<IPositionThresholdRule> Rules)
{
    public List<string> Validate(PositionThreshold item)
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

    public void RequireValid(PositionThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionThreshold> items)
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

/// <summary>Functions of position threshold rules.</summary>
public static class PositionThresholdRules
{
    public static PositionThresholdValidator DefaultPositionThresholdValidator()
    {
        return new PositionThresholdValidator(
            new List<IPositionThresholdRule>
            {
                new PositionThresholdAttemptsRangeRule(80, 86400),
                new PositionThresholdRequiredLabelRule("position threshold needs an entry"),
                new PositionThresholdRatingRangeRule(3.6, 4029.2),
                new PositionThresholdRequiredExternalRefRule("position threshold needs an entry"),
                new PositionThresholdCodeLengthRule(80),
                new PositionThresholdDiscountRangeRule(4.5, 2439.5),
            });
    }
}
