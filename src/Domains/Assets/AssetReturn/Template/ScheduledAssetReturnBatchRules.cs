namespace Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>A rule a scheduled asset return batch must pass. Check returns its problems, or none.</summary>
public interface IScheduledAssetReturnBatchRule
{
    List<string> Check(ScheduledAssetReturnBatch item);
}

public sealed record ScheduledAssetReturnBatchBudgetRangeRule(double Min, double Max) : IScheduledAssetReturnBatchRule
{
    public List<string> Check(ScheduledAssetReturnBatch item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnBatchNotesLengthRule(int MaxLength) : IScheduledAssetReturnBatchRule
{
    public List<string> Check(ScheduledAssetReturnBatch item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnBatchNameLengthRule(int MaxLength) : IScheduledAssetReturnBatchRule
{
    public List<string> Check(ScheduledAssetReturnBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnBatchGradeLengthRule(int MaxLength) : IScheduledAssetReturnBatchRule
{
    public List<string> Check(ScheduledAssetReturnBatch item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnBatchRequiredNameRule(string Message) : IScheduledAssetReturnBatchRule
{
    public List<string> Check(ScheduledAssetReturnBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled asset return batch.</summary>
public sealed record ScheduledAssetReturnBatchValidator(List<IScheduledAssetReturnBatchRule> Rules)
{
    public List<string> Validate(ScheduledAssetReturnBatch item)
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

    public void RequireValid(ScheduledAssetReturnBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAssetReturnBatch> items)
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

/// <summary>Functions of scheduled asset return batch rules.</summary>
public static class ScheduledAssetReturnBatchRules
{
    public static ScheduledAssetReturnBatchValidator DefaultScheduledAssetReturnBatchValidator()
    {
        return new ScheduledAssetReturnBatchValidator(
            new List<IScheduledAssetReturnBatchRule>
            {
                new ScheduledAssetReturnBatchBudgetRangeRule(8.6, 2612.9),
                new ScheduledAssetReturnBatchNotesLengthRule(80),
                new ScheduledAssetReturnBatchNameLengthRule(86400),
                new ScheduledAssetReturnBatchGradeLengthRule(86400),
                new ScheduledAssetReturnBatchRequiredNameRule("scheduled asset return batch needs an entry"),
            });
    }
}
