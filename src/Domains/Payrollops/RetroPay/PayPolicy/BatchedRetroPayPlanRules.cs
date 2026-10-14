namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

/// <summary>A rule a batched retro pay plan must pass. Check returns its problems, or none.</summary>
public interface IBatchedRetroPayPlanRule
{
    List<string> Check(BatchedRetroPayPlan item);
}

public sealed record BatchedRetroPayPlanManagerIdLengthRule(int MaxLength) : IBatchedRetroPayPlanRule
{
    public List<string> Check(BatchedRetroPayPlan item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRetroPayPlanCodePrefixRule(string Prefix) : IBatchedRetroPayPlanRule
{
    public List<string> Check(BatchedRetroPayPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRetroPayPlanRequiredManagerIdRule(string Message) : IBatchedRetroPayPlanRule
{
    public List<string> Check(BatchedRetroPayPlan item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched retro pay plan.</summary>
public sealed record BatchedRetroPayPlanValidator(List<IBatchedRetroPayPlanRule> Rules)
{
    public List<string> Validate(BatchedRetroPayPlan item)
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

    public void RequireValid(BatchedRetroPayPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRetroPayPlan> items)
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

/// <summary>Functions of batched retro pay plan rules.</summary>
public static class BatchedRetroPayPlanRules
{
    public static BatchedRetroPayPlanValidator DefaultBatchedRetroPayPlanValidator()
    {
        return new BatchedRetroPayPlanValidator(
            new List<IBatchedRetroPayPlanRule>
            {
                new BatchedRetroPayPlanManagerIdLengthRule(16),
                new BatchedRetroPayPlanCodePrefixRule("OPS"),
                new BatchedRetroPayPlanRequiredManagerIdRule("batched retro pay plan needs this field"),
            });
    }
}
