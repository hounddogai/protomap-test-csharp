namespace Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>A rule a position forecast must pass. Check returns its problems, or none.</summary>
public interface IPositionForecastRule
{
    List<string> Check(PositionForecast item);
}

public sealed record PositionForecastPriorityRangeRule(int Min, int Max) : IPositionForecastRule
{
    public List<string> Check(PositionForecast item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionForecastRequiredLabelRule(string Message) : IPositionForecastRule
{
    public List<string> Check(PositionForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionForecastRequiredTitleRule(string Message) : IPositionForecastRule
{
    public List<string> Check(PositionForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionForecastBatchSizeRangeRule(int Min, int Max) : IPositionForecastRule
{
    public List<string> Check(PositionForecast item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionForecastLabelLengthRule(int MaxLength) : IPositionForecastRule
{
    public List<string> Check(PositionForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position forecast.</summary>
public sealed record PositionForecastValidator(List<IPositionForecastRule> Rules)
{
    public List<string> Validate(PositionForecast item)
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

    public void RequireValid(PositionForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionForecast> items)
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

/// <summary>Functions of position forecast rules.</summary>
public static class PositionForecastRules
{
    public static PositionForecastValidator DefaultPositionForecastValidator()
    {
        return new PositionForecastValidator(
            new List<IPositionForecastRule>
            {
                new PositionForecastPriorityRangeRule(255, 3600),
                new PositionForecastRequiredLabelRule("position forecast needs this field"),
                new PositionForecastRequiredTitleRule("position forecast needs this field"),
                new PositionForecastBatchSizeRangeRule(64, 1000),
                new PositionForecastLabelLengthRule(32),
            });
    }
}
