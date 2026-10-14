namespace Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>A rule a batched asset return budget must pass. Check returns its problems, or none.</summary>
public interface IBatchedAssetReturnBudgetRule
{
    List<string> Check(BatchedAssetReturnBudget item);
}

public sealed record BatchedAssetReturnBudgetDescriptionLengthRule(int MaxLength) : IBatchedAssetReturnBudgetRule
{
    public List<string> Check(BatchedAssetReturnBudget item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAssetReturnBudgetActiveGuardRule(int Limit) : IBatchedAssetReturnBudgetRule
{
    public List<string> Check(BatchedAssetReturnBudget item)
    {
        if (item.Active && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAssetReturnBudgetCodePrefixRule(string Prefix) : IBatchedAssetReturnBudgetRule
{
    public List<string> Check(BatchedAssetReturnBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAssetReturnBudgetRequiredPhoneRule(string Message) : IBatchedAssetReturnBudgetRule
{
    public List<string> Check(BatchedAssetReturnBudget item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAssetReturnBudgetEndsAtRangeRule(int Min, int Max) : IBatchedAssetReturnBudgetRule
{
    public List<string> Check(BatchedAssetReturnBudget item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched asset return budget.</summary>
public sealed record BatchedAssetReturnBudgetValidator(List<IBatchedAssetReturnBudgetRule> Rules)
{
    public List<string> Validate(BatchedAssetReturnBudget item)
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

    public void RequireValid(BatchedAssetReturnBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAssetReturnBudget> items)
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

/// <summary>Functions of batched asset return budget rules.</summary>
public static class BatchedAssetReturnBudgetRules
{
    public static BatchedAssetReturnBudgetValidator DefaultBatchedAssetReturnBudgetValidator()
    {
        return new BatchedAssetReturnBudgetValidator(
            new List<IBatchedAssetReturnBudgetRule>
            {
                new BatchedAssetReturnBudgetDescriptionLengthRule(16),
                new BatchedAssetReturnBudgetActiveGuardRule(3600),
                new BatchedAssetReturnBudgetCodePrefixRule("ACM"),
                new BatchedAssetReturnBudgetRequiredPhoneRule("batched asset return budget needs a value"),
                new BatchedAssetReturnBudgetEndsAtRangeRule(120, 3600),
            });
    }
}
