namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>A rule a tiered desk metric must pass. Check returns its problems, or none.</summary>
public interface ITieredDeskMetricRule
{
    List<string> Check(TieredDeskMetric item);
}

public sealed record TieredDeskMetricRequiredGradeRule(string Message) : ITieredDeskMetricRule
{
    public List<string> Check(TieredDeskMetric item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredDeskMetricCodeLengthRule(int MaxLength) : ITieredDeskMetricRule
{
    public List<string> Check(TieredDeskMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeskMetricRatingRangeRule(double Min, double Max) : ITieredDeskMetricRule
{
    public List<string> Check(TieredDeskMetric item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeskMetricRequiredManagerIdRule(string Message) : ITieredDeskMetricRule
{
    public List<string> Check(TieredDeskMetric item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredDeskMetricCeilingRangeRule(double Min, double Max) : ITieredDeskMetricRule
{
    public List<string> Check(TieredDeskMetric item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredDeskMetricFloorValueRangeRule(double Min, double Max) : ITieredDeskMetricRule
{
    public List<string> Check(TieredDeskMetric item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered desk metric.</summary>
public sealed record TieredDeskMetricValidator(List<ITieredDeskMetricRule> Rules)
{
    public List<string> Validate(TieredDeskMetric item)
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

    public void RequireValid(TieredDeskMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredDeskMetric> items)
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

/// <summary>Functions of tiered desk metric rules.</summary>
public static class TieredDeskMetricRules
{
    public static TieredDeskMetricValidator DefaultTieredDeskMetricValidator()
    {
        return new TieredDeskMetricValidator(
            new List<ITieredDeskMetricRule>
            {
                new TieredDeskMetricRequiredGradeRule("tiered desk metric needs this field"),
                new TieredDeskMetricCodeLengthRule(32),
                new TieredDeskMetricRatingRangeRule(5.6, 2424.2),
                new TieredDeskMetricRequiredManagerIdRule("tiered desk metric needs this field"),
                new TieredDeskMetricCeilingRangeRule(2.7, 328.5),
                new TieredDeskMetricFloorValueRangeRule(8.6, 4743.7),
            });
    }
}
