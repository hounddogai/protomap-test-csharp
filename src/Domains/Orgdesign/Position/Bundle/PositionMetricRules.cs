namespace Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>A rule a position metric must pass. Check returns its problems, or none.</summary>
public interface IPositionMetricRule
{
    List<string> Check(PositionMetric item);
}

public sealed record PositionMetricCodeLengthRule(int MaxLength) : IPositionMetricRule
{
    public List<string> Check(PositionMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionMetricDepartmentLengthRule(int MaxLength) : IPositionMetricRule
{
    public List<string> Check(PositionMetric item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionMetricRequiredLabelRule(string Message) : IPositionMetricRule
{
    public List<string> Check(PositionMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionMetricRetriesRangeRule(int Min, int Max) : IPositionMetricRule
{
    public List<string> Check(PositionMetric item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position metric.</summary>
public sealed record PositionMetricValidator(List<IPositionMetricRule> Rules)
{
    public List<string> Validate(PositionMetric item)
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

    public void RequireValid(PositionMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionMetric> items)
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

/// <summary>Functions of position metric rules.</summary>
public static class PositionMetricRules
{
    public static PositionMetricValidator DefaultPositionMetricValidator()
    {
        return new PositionMetricValidator(
            new List<IPositionMetricRule>
            {
                new PositionMetricCodeLengthRule(32),
                new PositionMetricDepartmentLengthRule(255),
                new PositionMetricRequiredLabelRule("position metric needs a value"),
                new PositionMetricRetriesRangeRule(64, 3600),
            });
    }
}
