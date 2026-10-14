namespace Acme.Workforce.Assets.Assignment.Batch;

/// <summary>A rule a default assignment batch must pass. Check returns its problems, or none.</summary>
public interface IDefaultAssignmentBatchRule
{
    List<string> Check(DefaultAssignmentBatch item);
}

public sealed record DefaultAssignmentBatchRequiredNameRule(string Message) : IDefaultAssignmentBatchRule
{
    public List<string> Check(DefaultAssignmentBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAssignmentBatchCodePrefixRule(string Prefix) : IDefaultAssignmentBatchRule
{
    public List<string> Check(DefaultAssignmentBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAssignmentBatchQuantityRangeRule(int Min, int Max) : IDefaultAssignmentBatchRule
{
    public List<string> Check(DefaultAssignmentBatch item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default assignment batch.</summary>
public sealed record DefaultAssignmentBatchValidator(List<IDefaultAssignmentBatchRule> Rules)
{
    public List<string> Validate(DefaultAssignmentBatch item)
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

    public void RequireValid(DefaultAssignmentBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAssignmentBatch> items)
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

/// <summary>Functions of default assignment batch rules.</summary>
public static class DefaultAssignmentBatchRules
{
    public static DefaultAssignmentBatchValidator DefaultDefaultAssignmentBatchValidator()
    {
        return new DefaultAssignmentBatchValidator(
            new List<IDefaultAssignmentBatchRule>
            {
                new DefaultAssignmentBatchRequiredNameRule("default assignment batch needs a value"),
                new DefaultAssignmentBatchCodePrefixRule("Q"),
                new DefaultAssignmentBatchQuantityRangeRule(255, 3600),
            });
    }
}
