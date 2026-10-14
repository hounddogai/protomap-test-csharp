namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>A rule a return plan checkpoint must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanCheckpointRule
{
    List<string> Check(ReturnPlanCheckpoint item);
}

public sealed record ReturnPlanCheckpointRequiredCodeRule(string Message) : IReturnPlanCheckpointRule
{
    public List<string> Check(ReturnPlanCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanCheckpointTitleLengthRule(int MaxLength) : IReturnPlanCheckpointRule
{
    public List<string> Check(ReturnPlanCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanCheckpointRequiredManagerIdRule(string Message) : IReturnPlanCheckpointRule
{
    public List<string> Check(ReturnPlanCheckpoint item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanCheckpointCodeLengthRule(int MaxLength) : IReturnPlanCheckpointRule
{
    public List<string> Check(ReturnPlanCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan checkpoint.</summary>
public sealed record ReturnPlanCheckpointValidator(List<IReturnPlanCheckpointRule> Rules)
{
    public List<string> Validate(ReturnPlanCheckpoint item)
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

    public void RequireValid(ReturnPlanCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanCheckpoint> items)
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

/// <summary>Functions of return plan checkpoint rules.</summary>
public static class ReturnPlanCheckpointRules
{
    public static ReturnPlanCheckpointValidator DefaultReturnPlanCheckpointValidator()
    {
        return new ReturnPlanCheckpointValidator(
            new List<IReturnPlanCheckpointRule>
            {
                new ReturnPlanCheckpointRequiredCodeRule("return plan checkpoint needs a value"),
                new ReturnPlanCheckpointTitleLengthRule(32),
                new ReturnPlanCheckpointRequiredManagerIdRule("return plan checkpoint needs a value"),
                new ReturnPlanCheckpointCodeLengthRule(86400),
            });
    }
}
