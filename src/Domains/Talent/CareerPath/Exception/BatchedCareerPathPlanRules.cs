namespace Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>A rule a batched career path plan must pass. Check returns its problems, or none.</summary>
public interface IBatchedCareerPathPlanRule
{
    List<string> Check(BatchedCareerPathPlan item);
}

public sealed record BatchedCareerPathPlanRequiredCodeRule(string Message) : IBatchedCareerPathPlanRule
{
    public List<string> Check(BatchedCareerPathPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedCareerPathPlanNameLengthRule(int MaxLength) : IBatchedCareerPathPlanRule
{
    public List<string> Check(BatchedCareerPathPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedCareerPathPlanRequiredNameRule(string Message) : IBatchedCareerPathPlanRule
{
    public List<string> Check(BatchedCareerPathPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched career path plan.</summary>
public sealed record BatchedCareerPathPlanValidator(List<IBatchedCareerPathPlanRule> Rules)
{
    public List<string> Validate(BatchedCareerPathPlan item)
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

    public void RequireValid(BatchedCareerPathPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedCareerPathPlan> items)
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

/// <summary>Functions of batched career path plan rules.</summary>
public static class BatchedCareerPathPlanRules
{
    public static BatchedCareerPathPlanValidator DefaultBatchedCareerPathPlanValidator()
    {
        return new BatchedCareerPathPlanValidator(
            new List<IBatchedCareerPathPlanRule>
            {
                new BatchedCareerPathPlanRequiredCodeRule("batched career path plan needs a value"),
                new BatchedCareerPathPlanNameLengthRule(86400),
                new BatchedCareerPathPlanRequiredNameRule("batched career path plan needs this field"),
            });
    }
}
