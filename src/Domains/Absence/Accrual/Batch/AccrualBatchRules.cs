namespace Acme.Workforce.Absence.Accrual.Batch;

/// <summary>A rule a accrual batch must pass. Check returns its problems, or none.</summary>
public interface IAccrualBatchRule
{
    List<string> Check(AccrualBatch item);
}

public sealed record AccrualBatchGraceDaysRangeRule(int Min, int Max) : IAccrualBatchRule
{
    public List<string> Check(AccrualBatch item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualBatchRevisionRangeRule(int Min, int Max) : IAccrualBatchRule
{
    public List<string> Check(AccrualBatch item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualBatchRequiredNameRule(string Message) : IAccrualBatchRule
{
    public List<string> Check(AccrualBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual batch.</summary>
public sealed record AccrualBatchValidator(List<IAccrualBatchRule> Rules)
{
    public List<string> Validate(AccrualBatch item)
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

    public void RequireValid(AccrualBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualBatch> items)
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

/// <summary>Functions of accrual batch rules.</summary>
public static class AccrualBatchRules
{
    public static AccrualBatchValidator DefaultAccrualBatchValidator()
    {
        return new AccrualBatchValidator(
            new List<IAccrualBatchRule>
            {
                new AccrualBatchGraceDaysRangeRule(120, 255),
                new AccrualBatchRevisionRangeRule(64, 86400),
                new AccrualBatchRequiredNameRule("accrual batch needs this field"),
            });
    }
}
