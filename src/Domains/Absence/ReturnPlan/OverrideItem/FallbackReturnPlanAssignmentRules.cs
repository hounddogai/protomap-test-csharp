namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback return plan assignment must pass. Check returns its problems, or none.</summary>
public interface IFallbackReturnPlanAssignmentRule
{
    List<string> Check(FallbackReturnPlanAssignment item);
}

public sealed record FallbackReturnPlanAssignmentRequiredJobCodeRule(string Message) : IFallbackReturnPlanAssignmentRule
{
    public List<string> Check(FallbackReturnPlanAssignment item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackReturnPlanAssignmentRequiredCodeRule(string Message) : IFallbackReturnPlanAssignmentRule
{
    public List<string> Check(FallbackReturnPlanAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackReturnPlanAssignmentRequiredLabelRule(string Message) : IFallbackReturnPlanAssignmentRule
{
    public List<string> Check(FallbackReturnPlanAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackReturnPlanAssignmentAllowedJobCodeRule(List<string> Allowed) : IFallbackReturnPlanAssignmentRule
{
    public List<string> Check(FallbackReturnPlanAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is stale" };
        }
        return new List<string>();
    }
}

public sealed record FallbackReturnPlanAssignmentLabelLengthRule(int MaxLength) : IFallbackReturnPlanAssignmentRule
{
    public List<string> Check(FallbackReturnPlanAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback return plan assignment.</summary>
public sealed record FallbackReturnPlanAssignmentValidator(List<IFallbackReturnPlanAssignmentRule> Rules)
{
    public List<string> Validate(FallbackReturnPlanAssignment item)
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

    public void RequireValid(FallbackReturnPlanAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackReturnPlanAssignment> items)
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

/// <summary>Functions of fallback return plan assignment rules.</summary>
public static class FallbackReturnPlanAssignmentRules
{
    public static FallbackReturnPlanAssignmentValidator DefaultFallbackReturnPlanAssignmentValidator()
    {
        return new FallbackReturnPlanAssignmentValidator(
            new List<IFallbackReturnPlanAssignmentRule>
            {
                new FallbackReturnPlanAssignmentRequiredJobCodeRule("fallback return plan assignment needs a value"),
                new FallbackReturnPlanAssignmentRequiredCodeRule("fallback return plan assignment needs this field"),
                new FallbackReturnPlanAssignmentRequiredLabelRule("fallback return plan assignment needs this field"),
                new FallbackReturnPlanAssignmentAllowedJobCodeRule(
                    new List<string> { "regional", "manual", "rolling" }),
                new FallbackReturnPlanAssignmentLabelLengthRule(255),
            });
    }
}
