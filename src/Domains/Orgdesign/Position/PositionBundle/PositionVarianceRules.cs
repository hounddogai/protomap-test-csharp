namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>A rule a position variance must pass. Check returns its problems, or none.</summary>
public interface IPositionVarianceRule
{
    List<string> Check(PositionVariance item);
}

public sealed record PositionVarianceRequiredCodeRule(string Message) : IPositionVarianceRule
{
    public List<string> Check(PositionVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionVarianceWindowDaysRangeRule(int Min, int Max) : IPositionVarianceRule
{
    public List<string> Check(PositionVariance item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionVarianceEndsAtRangeRule(int Min, int Max) : IPositionVarianceRule
{
    public List<string> Check(PositionVariance item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionVarianceCodeLengthRule(int MaxLength) : IPositionVarianceRule
{
    public List<string> Check(PositionVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position variance.</summary>
public sealed record PositionVarianceValidator(List<IPositionVarianceRule> Rules)
{
    public List<string> Validate(PositionVariance item)
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

    public void RequireValid(PositionVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionVariance> items)
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

/// <summary>Functions of position variance rules.</summary>
public static class PositionVarianceRules
{
    public static PositionVarianceValidator DefaultPositionVarianceValidator()
    {
        return new PositionVarianceValidator(
            new List<IPositionVarianceRule>
            {
                new PositionVarianceRequiredCodeRule("position variance needs an entry"),
                new PositionVarianceWindowDaysRangeRule(16, 32),
                new PositionVarianceEndsAtRangeRule(80, 1000),
                new PositionVarianceCodeLengthRule(1000),
            });
    }
}
