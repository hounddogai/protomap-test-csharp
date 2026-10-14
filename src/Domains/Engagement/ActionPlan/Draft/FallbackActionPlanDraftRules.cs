namespace Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>A rule a fallback action plan draft must pass. Check returns its problems, or none.</summary>
public interface IFallbackActionPlanDraftRule
{
    List<string> Check(FallbackActionPlanDraft item);
}

public sealed record FallbackActionPlanDraftRequiredLabelRule(string Message) : IFallbackActionPlanDraftRule
{
    public List<string> Check(FallbackActionPlanDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackActionPlanDraftCodeLengthRule(int MaxLength) : IFallbackActionPlanDraftRule
{
    public List<string> Check(FallbackActionPlanDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackActionPlanDraftCodePrefixRule(string Prefix) : IFallbackActionPlanDraftRule
{
    public List<string> Check(FallbackActionPlanDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback action plan draft.</summary>
public sealed record FallbackActionPlanDraftValidator(List<IFallbackActionPlanDraftRule> Rules)
{
    public List<string> Validate(FallbackActionPlanDraft item)
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

    public void RequireValid(FallbackActionPlanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackActionPlanDraft> items)
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

/// <summary>Functions of fallback action plan draft rules.</summary>
public static class FallbackActionPlanDraftRules
{
    public static FallbackActionPlanDraftValidator DefaultFallbackActionPlanDraftValidator()
    {
        return new FallbackActionPlanDraftValidator(
            new List<IFallbackActionPlanDraftRule>
            {
                new FallbackActionPlanDraftRequiredLabelRule("fallback action plan draft needs this field"),
                new FallbackActionPlanDraftCodeLengthRule(255),
                new FallbackActionPlanDraftCodePrefixRule("Q"),
            });
    }
}
