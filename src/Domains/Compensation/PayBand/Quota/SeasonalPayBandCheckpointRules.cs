namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal pay band checkpoint must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPayBandCheckpointRule
{
    List<string> Check(SeasonalPayBandCheckpoint item);
}

public sealed record SeasonalPayBandCheckpointRequiredSourceRule(string Message) : ISeasonalPayBandCheckpointRule
{
    public List<string> Check(SeasonalPayBandCheckpoint item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandCheckpointMarginRangeRule(double Min, double Max) : ISeasonalPayBandCheckpointRule
{
    public List<string> Check(SeasonalPayBandCheckpoint item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalPayBandCheckpointAllowedCodeRule(List<string> Allowed) : ISeasonalPayBandCheckpointRule
{
    public List<string> Check(SeasonalPayBandCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandCheckpointRequiredCodeRule(string Message) : ISeasonalPayBandCheckpointRule
{
    public List<string> Check(SeasonalPayBandCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandCheckpointCodeLengthRule(int MaxLength) : ISeasonalPayBandCheckpointRule
{
    public List<string> Check(SeasonalPayBandCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal pay band checkpoint.</summary>
public sealed record SeasonalPayBandCheckpointValidator(List<ISeasonalPayBandCheckpointRule> Rules)
{
    public List<string> Validate(SeasonalPayBandCheckpoint item)
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

    public void RequireValid(SeasonalPayBandCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPayBandCheckpoint> items)
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

/// <summary>Functions of seasonal pay band checkpoint rules.</summary>
public static class SeasonalPayBandCheckpointRules
{
    public static SeasonalPayBandCheckpointValidator DefaultSeasonalPayBandCheckpointValidator()
    {
        return new SeasonalPayBandCheckpointValidator(
            new List<ISeasonalPayBandCheckpointRule>
            {
                new SeasonalPayBandCheckpointRequiredSourceRule("seasonal pay band checkpoint needs this field"),
                new SeasonalPayBandCheckpointMarginRangeRule(1.1, 847.3),
                new SeasonalPayBandCheckpointAllowedCodeRule(new List<string>()),
                new SeasonalPayBandCheckpointRequiredCodeRule("seasonal pay band checkpoint needs this field"),
                new SeasonalPayBandCheckpointCodeLengthRule(86400),
            });
    }
}
