namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback career path adjustment must pass. Check returns its problems, or none.</summary>
public interface IFallbackCareerPathAdjustmentRule
{
    List<string> Check(FallbackCareerPathAdjustment item);
}

public sealed record FallbackCareerPathAdjustmentManagerIdLengthRule(int MaxLength) : IFallbackCareerPathAdjustmentRule
{
    public List<string> Check(FallbackCareerPathAdjustment item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCareerPathAdjustmentAllowedLabelRule(List<string> Allowed) : IFallbackCareerPathAdjustmentRule
{
    public List<string> Check(FallbackCareerPathAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCareerPathAdjustmentEndsAtRangeRule(int Min, int Max) : IFallbackCareerPathAdjustmentRule
{
    public List<string> Check(FallbackCareerPathAdjustment item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback career path adjustment.</summary>
public sealed record FallbackCareerPathAdjustmentValidator(List<IFallbackCareerPathAdjustmentRule> Rules)
{
    public List<string> Validate(FallbackCareerPathAdjustment item)
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

    public void RequireValid(FallbackCareerPathAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackCareerPathAdjustment> items)
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

/// <summary>Functions of fallback career path adjustment rules.</summary>
public static class FallbackCareerPathAdjustmentRules
{
    public static FallbackCareerPathAdjustmentValidator DefaultFallbackCareerPathAdjustmentValidator()
    {
        return new FallbackCareerPathAdjustmentValidator(
            new List<IFallbackCareerPathAdjustmentRule>
            {
                new FallbackCareerPathAdjustmentManagerIdLengthRule(80),
                new FallbackCareerPathAdjustmentAllowedLabelRule(
                    new List<string> { "default", "pending", "batched" }),
                new FallbackCareerPathAdjustmentEndsAtRangeRule(64, 64),
            });
    }
}
