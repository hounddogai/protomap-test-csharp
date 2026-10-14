namespace Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>A rule a visitor batch must pass. Check returns its problems, or none.</summary>
public interface IVisitorBatchRule
{
    List<string> Check(VisitorBatch item);
}

public sealed record VisitorBatchWindowDaysRangeRule(int Min, int Max) : IVisitorBatchRule
{
    public List<string> Check(VisitorBatch item)
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

public sealed record VisitorBatchTitleLengthRule(int MaxLength) : IVisitorBatchRule
{
    public List<string> Check(VisitorBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorBatchRevisionRangeRule(int Min, int Max) : IVisitorBatchRule
{
    public List<string> Check(VisitorBatch item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor batch.</summary>
public sealed record VisitorBatchValidator(List<IVisitorBatchRule> Rules)
{
    public List<string> Validate(VisitorBatch item)
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

    public void RequireValid(VisitorBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorBatch> items)
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

/// <summary>Functions of visitor batch rules.</summary>
public static class VisitorBatchRules
{
    public static VisitorBatchValidator DefaultVisitorBatchValidator()
    {
        return new VisitorBatchValidator(
            new List<IVisitorBatchRule>
            {
                new VisitorBatchWindowDaysRangeRule(16, 1000),
                new VisitorBatchTitleLengthRule(1000),
                new VisitorBatchRevisionRangeRule(120, 1000),
            });
    }
}
