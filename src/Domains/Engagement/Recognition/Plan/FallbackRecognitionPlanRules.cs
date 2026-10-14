namespace Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>A rule a fallback recognition plan must pass. Check returns its problems, or none.</summary>
public interface IFallbackRecognitionPlanRule
{
    List<string> Check(FallbackRecognitionPlan item);
}

public sealed record FallbackRecognitionPlanLabelLengthRule(int MaxLength) : IFallbackRecognitionPlanRule
{
    public List<string> Check(FallbackRecognitionPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionPlanRequiredCodeRule(string Message) : IFallbackRecognitionPlanRule
{
    public List<string> Check(FallbackRecognitionPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionPlanCodeLengthRule(int MaxLength) : IFallbackRecognitionPlanRule
{
    public List<string> Check(FallbackRecognitionPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionPlanOwnerIdLengthRule(int MaxLength) : IFallbackRecognitionPlanRule
{
    public List<string> Check(FallbackRecognitionPlan item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback recognition plan.</summary>
public sealed record FallbackRecognitionPlanValidator(List<IFallbackRecognitionPlanRule> Rules)
{
    public List<string> Validate(FallbackRecognitionPlan item)
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

    public void RequireValid(FallbackRecognitionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRecognitionPlan> items)
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

/// <summary>Functions of fallback recognition plan rules.</summary>
public static class FallbackRecognitionPlanRules
{
    public static FallbackRecognitionPlanValidator DefaultFallbackRecognitionPlanValidator()
    {
        return new FallbackRecognitionPlanValidator(
            new List<IFallbackRecognitionPlanRule>
            {
                new FallbackRecognitionPlanLabelLengthRule(80),
                new FallbackRecognitionPlanRequiredCodeRule("fallback recognition plan needs this field"),
                new FallbackRecognitionPlanCodeLengthRule(120),
                new FallbackRecognitionPlanOwnerIdLengthRule(80),
            });
    }
}
