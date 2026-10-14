namespace Acme.Workforce.Compliance.Exception.Plan;

/// <summary>A rule a pending exception plan must pass. Check returns its problems, or none.</summary>
public interface IPendingExceptionPlanRule
{
    List<string> Check(PendingExceptionPlan item);
}

public sealed record PendingExceptionPlanTitleLengthRule(int MaxLength) : IPendingExceptionPlanRule
{
    public List<string> Check(PendingExceptionPlan item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionPlanRequiredCodeRule(string Message) : IPendingExceptionPlanRule
{
    public List<string> Check(PendingExceptionPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionPlanCodeLengthRule(int MaxLength) : IPendingExceptionPlanRule
{
    public List<string> Check(PendingExceptionPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending exception plan.</summary>
public sealed record PendingExceptionPlanValidator(List<IPendingExceptionPlanRule> Rules)
{
    public List<string> Validate(PendingExceptionPlan item)
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

    public void RequireValid(PendingExceptionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingExceptionPlan> items)
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

/// <summary>Functions of pending exception plan rules.</summary>
public static class PendingExceptionPlanRules
{
    public static PendingExceptionPlanValidator DefaultPendingExceptionPlanValidator()
    {
        return new PendingExceptionPlanValidator(
            new List<IPendingExceptionPlanRule>
            {
                new PendingExceptionPlanTitleLengthRule(1000),
                new PendingExceptionPlanRequiredCodeRule("pending exception plan needs a value"),
                new PendingExceptionPlanCodeLengthRule(16),
            });
    }
}
