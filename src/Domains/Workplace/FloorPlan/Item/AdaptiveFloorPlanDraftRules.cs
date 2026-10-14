namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive floor plan draft must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveFloorPlanDraftRule
{
    List<string> Check(AdaptiveFloorPlanDraft item);
}

public sealed record AdaptiveFloorPlanDraftChecksumLengthRule(int MaxLength) : IAdaptiveFloorPlanDraftRule
{
    public List<string> Check(AdaptiveFloorPlanDraft item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanDraftAllowedNameRule(List<string> Allowed) : IAdaptiveFloorPlanDraftRule
{
    public List<string> Check(AdaptiveFloorPlanDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanDraftRequiredNameRule(string Message) : IAdaptiveFloorPlanDraftRule
{
    public List<string> Check(AdaptiveFloorPlanDraft item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanDraftCodePrefixRule(string Prefix) : IAdaptiveFloorPlanDraftRule
{
    public List<string> Check(AdaptiveFloorPlanDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanDraftNameLengthRule(int MaxLength) : IAdaptiveFloorPlanDraftRule
{
    public List<string> Check(AdaptiveFloorPlanDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanDraftLimitRangeRule(int Min, int Max) : IAdaptiveFloorPlanDraftRule
{
    public List<string> Check(AdaptiveFloorPlanDraft item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive floor plan draft.</summary>
public sealed record AdaptiveFloorPlanDraftValidator(List<IAdaptiveFloorPlanDraftRule> Rules)
{
    public List<string> Validate(AdaptiveFloorPlanDraft item)
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

    public void RequireValid(AdaptiveFloorPlanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveFloorPlanDraft> items)
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

/// <summary>Functions of adaptive floor plan draft rules.</summary>
public static class AdaptiveFloorPlanDraftRules
{
    public static AdaptiveFloorPlanDraftValidator DefaultAdaptiveFloorPlanDraftValidator()
    {
        return new AdaptiveFloorPlanDraftValidator(
            new List<IAdaptiveFloorPlanDraftRule>
            {
                new AdaptiveFloorPlanDraftChecksumLengthRule(120),
                new AdaptiveFloorPlanDraftAllowedNameRule(new List<string> { "fallback", "global", "primary" }),
                new AdaptiveFloorPlanDraftRequiredNameRule("adaptive floor plan draft needs this field"),
                new AdaptiveFloorPlanDraftCodePrefixRule("Q"),
                new AdaptiveFloorPlanDraftNameLengthRule(16),
                new AdaptiveFloorPlanDraftLimitRangeRule(80, 3600),
            });
    }
}
