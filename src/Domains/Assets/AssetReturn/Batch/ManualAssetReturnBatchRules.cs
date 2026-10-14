namespace Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>A rule a manual asset return batch must pass. Check returns its problems, or none.</summary>
public interface IManualAssetReturnBatchRule
{
    List<string> Check(ManualAssetReturnBatch item);
}

public sealed record ManualAssetReturnBatchLabelLengthRule(int MaxLength) : IManualAssetReturnBatchRule
{
    public List<string> Check(ManualAssetReturnBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnBatchRequiredLabelRule(string Message) : IManualAssetReturnBatchRule
{
    public List<string> Check(ManualAssetReturnBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnBatchCodeLengthRule(int MaxLength) : IManualAssetReturnBatchRule
{
    public List<string> Check(ManualAssetReturnBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnBatchRequiredEmployeeIdRule(string Message) : IManualAssetReturnBatchRule
{
    public List<string> Check(ManualAssetReturnBatch item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual asset return batch.</summary>
public sealed record ManualAssetReturnBatchValidator(List<IManualAssetReturnBatchRule> Rules)
{
    public List<string> Validate(ManualAssetReturnBatch item)
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

    public void RequireValid(ManualAssetReturnBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAssetReturnBatch> items)
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

/// <summary>Functions of manual asset return batch rules.</summary>
public static class ManualAssetReturnBatchRules
{
    public static ManualAssetReturnBatchValidator DefaultManualAssetReturnBatchValidator()
    {
        return new ManualAssetReturnBatchValidator(
            new List<IManualAssetReturnBatchRule>
            {
                new ManualAssetReturnBatchLabelLengthRule(255),
                new ManualAssetReturnBatchRequiredLabelRule("manual asset return batch needs a value"),
                new ManualAssetReturnBatchCodeLengthRule(32),
                new ManualAssetReturnBatchRequiredEmployeeIdRule("manual asset return batch needs this field"),
            });
    }
}
