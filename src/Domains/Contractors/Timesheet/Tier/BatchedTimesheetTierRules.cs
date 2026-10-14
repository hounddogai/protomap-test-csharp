namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched timesheet tier must pass. Check returns its problems, or none.</summary>
public interface IBatchedTimesheetTierRule
{
    List<string> Check(BatchedTimesheetTier item);
}

public sealed record BatchedTimesheetTierAllowedCurrencyRule(List<string> Allowed) : IBatchedTimesheetTierRule
{
    public List<string> Check(BatchedTimesheetTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTimesheetTierCodeLengthRule(int MaxLength) : IBatchedTimesheetTierRule
{
    public List<string> Check(BatchedTimesheetTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTimesheetTierRequiredCodeRule(string Message) : IBatchedTimesheetTierRule
{
    public List<string> Check(BatchedTimesheetTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTimesheetTierMinimumRangeRule(double Min, double Max) : IBatchedTimesheetTierRule
{
    public List<string> Check(BatchedTimesheetTier item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTimesheetTierRequiredLabelRule(string Message) : IBatchedTimesheetTierRule
{
    public List<string> Check(BatchedTimesheetTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched timesheet tier.</summary>
public sealed record BatchedTimesheetTierValidator(List<IBatchedTimesheetTierRule> Rules)
{
    public List<string> Validate(BatchedTimesheetTier item)
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

    public void RequireValid(BatchedTimesheetTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTimesheetTier> items)
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

/// <summary>Functions of batched timesheet tier rules.</summary>
public static class BatchedTimesheetTierRules
{
    public static BatchedTimesheetTierValidator DefaultBatchedTimesheetTierValidator()
    {
        return new BatchedTimesheetTierValidator(
            new List<IBatchedTimesheetTierRule>
            {
                new BatchedTimesheetTierAllowedCurrencyRule(new List<string> { "regional", "capped", "legacy" }),
                new BatchedTimesheetTierCodeLengthRule(120),
                new BatchedTimesheetTierRequiredCodeRule("batched timesheet tier needs a value"),
                new BatchedTimesheetTierMinimumRangeRule(8.9, 3203.0),
                new BatchedTimesheetTierRequiredLabelRule("batched timesheet tier needs a value"),
            });
    }
}
