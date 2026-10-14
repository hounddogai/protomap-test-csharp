namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

/// <summary>A rule a strict statement of work metric must pass. Check returns its problems, or none.</summary>
public interface IStrictStatementOfWorkMetricRule
{
    List<string> Check(StrictStatementOfWorkMetric item);
}

public sealed record StrictStatementOfWorkMetricCodeLengthRule(int MaxLength) : IStrictStatementOfWorkMetricRule
{
    public List<string> Check(StrictStatementOfWorkMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictStatementOfWorkMetricRequiredExternalRefRule(string Message) : IStrictStatementOfWorkMetricRule
{
    public List<string> Check(StrictStatementOfWorkMetric item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictStatementOfWorkMetricUpdatedAtRangeRule(int Min, int Max) : IStrictStatementOfWorkMetricRule
{
    public List<string> Check(StrictStatementOfWorkMetric item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict statement of work metric.</summary>
public sealed record StrictStatementOfWorkMetricValidator(List<IStrictStatementOfWorkMetricRule> Rules)
{
    public List<string> Validate(StrictStatementOfWorkMetric item)
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

    public void RequireValid(StrictStatementOfWorkMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictStatementOfWorkMetric> items)
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

/// <summary>Functions of strict statement of work metric rules.</summary>
public static class StrictStatementOfWorkMetricRules
{
    public static StrictStatementOfWorkMetricValidator DefaultStrictStatementOfWorkMetricValidator()
    {
        return new StrictStatementOfWorkMetricValidator(
            new List<IStrictStatementOfWorkMetricRule>
            {
                new StrictStatementOfWorkMetricCodeLengthRule(1000),
                new StrictStatementOfWorkMetricRequiredExternalRefRule(
                    "strict statement of work metric needs a value"),
                new StrictStatementOfWorkMetricUpdatedAtRangeRule(16, 255),
            });
    }
}
