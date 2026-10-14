namespace Acme.Workforce.Workplace.Desk.Budget;

/// <summary>A rule a desk batch must pass. Check returns its problems, or none.</summary>
public interface IDeskBatchRule
{
    List<string> Check(DeskBatch item);
}

public sealed record DeskBatchLeaveDaysRangeRule(int Min, int Max) : IDeskBatchRule
{
    public List<string> Check(DeskBatch item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskBatchUnitLengthRule(int MaxLength) : IDeskBatchRule
{
    public List<string> Check(DeskBatch item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskBatchMaximumRangeRule(double Min, double Max) : IDeskBatchRule
{
    public List<string> Check(DeskBatch item)
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

/// <summary>Runs every rule of a desk batch.</summary>
public sealed record DeskBatchValidator(List<IDeskBatchRule> Rules)
{
    public List<string> Validate(DeskBatch item)
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

    public void RequireValid(DeskBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskBatch> items)
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

/// <summary>Functions of desk batch rules.</summary>
public static class DeskBatchRules
{
    public static DeskBatchValidator DefaultDeskBatchValidator()
    {
        return new DeskBatchValidator(
            new List<IDeskBatchRule>
            {
                new DeskBatchLeaveDaysRangeRule(120, 255),
                new DeskBatchUnitLengthRule(255),
                new DeskBatchMaximumRangeRule(5.2, 2401.5),
            });
    }
}
