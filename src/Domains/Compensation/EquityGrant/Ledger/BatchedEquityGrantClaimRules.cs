namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

/// <summary>A rule a batched equity grant claim must pass. Check returns its problems, or none.</summary>
public interface IBatchedEquityGrantClaimRule
{
    List<string> Check(BatchedEquityGrantClaim item);
}

public sealed record BatchedEquityGrantClaimFloorValueRangeRule(double Min, double Max) : IBatchedEquityGrantClaimRule
{
    public List<string> Check(BatchedEquityGrantClaim item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedEquityGrantClaimGraceDaysRangeRule(int Min, int Max) : IBatchedEquityGrantClaimRule
{
    public List<string> Check(BatchedEquityGrantClaim item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedEquityGrantClaimOwnerIdLengthRule(int MaxLength) : IBatchedEquityGrantClaimRule
{
    public List<string> Check(BatchedEquityGrantClaim item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedEquityGrantClaimRequiredCodeRule(string Message) : IBatchedEquityGrantClaimRule
{
    public List<string> Check(BatchedEquityGrantClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched equity grant claim.</summary>
public sealed record BatchedEquityGrantClaimValidator(List<IBatchedEquityGrantClaimRule> Rules)
{
    public List<string> Validate(BatchedEquityGrantClaim item)
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

    public void RequireValid(BatchedEquityGrantClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedEquityGrantClaim> items)
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

/// <summary>Functions of batched equity grant claim rules.</summary>
public static class BatchedEquityGrantClaimRules
{
    public static BatchedEquityGrantClaimValidator DefaultBatchedEquityGrantClaimValidator()
    {
        return new BatchedEquityGrantClaimValidator(
            new List<IBatchedEquityGrantClaimRule>
            {
                new BatchedEquityGrantClaimFloorValueRangeRule(0.9, 3826.5),
                new BatchedEquityGrantClaimGraceDaysRangeRule(32, 3600),
                new BatchedEquityGrantClaimOwnerIdLengthRule(255),
                new BatchedEquityGrantClaimRequiredCodeRule("batched equity grant claim needs this field"),
            });
    }
}
