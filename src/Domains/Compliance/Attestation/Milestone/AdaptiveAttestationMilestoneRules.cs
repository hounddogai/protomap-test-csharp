namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive attestation milestone must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAttestationMilestoneRule
{
    List<string> Check(AdaptiveAttestationMilestone item);
}

public sealed record AdaptiveAttestationMilestoneRequiredLocationRule(string Message) : IAdaptiveAttestationMilestoneRule
{
    public List<string> Check(AdaptiveAttestationMilestone item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationMilestoneCeilingRangeRule(double Min, double Max) : IAdaptiveAttestationMilestoneRule
{
    public List<string> Check(AdaptiveAttestationMilestone item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveAttestationMilestoneCostRangeRule(double Min, double Max) : IAdaptiveAttestationMilestoneRule
{
    public List<string> Check(AdaptiveAttestationMilestone item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveAttestationMilestoneRequiredGradeRule(string Message) : IAdaptiveAttestationMilestoneRule
{
    public List<string> Check(AdaptiveAttestationMilestone item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationMilestoneAllowedLocationRule(List<string> Allowed) : IAdaptiveAttestationMilestoneRule
{
    public List<string> Check(AdaptiveAttestationMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationMilestoneRequiredCodeRule(string Message) : IAdaptiveAttestationMilestoneRule
{
    public List<string> Check(AdaptiveAttestationMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive attestation milestone.</summary>
public sealed record AdaptiveAttestationMilestoneValidator(List<IAdaptiveAttestationMilestoneRule> Rules)
{
    public List<string> Validate(AdaptiveAttestationMilestone item)
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

    public void RequireValid(AdaptiveAttestationMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAttestationMilestone> items)
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

/// <summary>Functions of adaptive attestation milestone rules.</summary>
public static class AdaptiveAttestationMilestoneRules
{
    public static AdaptiveAttestationMilestoneValidator DefaultAdaptiveAttestationMilestoneValidator()
    {
        return new AdaptiveAttestationMilestoneValidator(
            new List<IAdaptiveAttestationMilestoneRule>
            {
                new AdaptiveAttestationMilestoneRequiredLocationRule("adaptive attestation milestone needs a value"),
                new AdaptiveAttestationMilestoneCeilingRangeRule(4.6, 3628.8),
                new AdaptiveAttestationMilestoneCostRangeRule(9.9, 3778.4),
                new AdaptiveAttestationMilestoneRequiredGradeRule("adaptive attestation milestone needs this field"),
                new AdaptiveAttestationMilestoneAllowedLocationRule(new List<string> { "fallback", "capped" }),
                new AdaptiveAttestationMilestoneRequiredCodeRule("adaptive attestation milestone needs a value"),
            });
    }
}
