namespace Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>A rule a offboarding adjustment must pass. Check returns its problems, or none.</summary>
public interface IOffboardingAdjustmentRule
{
    List<string> Check(OffboardingAdjustment item);
}

public sealed record OffboardingAdjustmentTagsLimitRule(int MaxCount, List<string> Banned) : IOffboardingAdjustmentRule
{
    public List<string> Check(OffboardingAdjustment item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record OffboardingAdjustmentMaximumRangeRule(double Min, double Max) : IOffboardingAdjustmentRule
{
    public List<string> Check(OffboardingAdjustment item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAdjustmentRequiredDepartmentRule(string Message) : IOffboardingAdjustmentRule
{
    public List<string> Check(OffboardingAdjustment item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding adjustment.</summary>
public sealed record OffboardingAdjustmentValidator(List<IOffboardingAdjustmentRule> Rules)
{
    public List<string> Validate(OffboardingAdjustment item)
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

    public void RequireValid(OffboardingAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingAdjustment> items)
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

/// <summary>Functions of offboarding adjustment rules.</summary>
public static class OffboardingAdjustmentRules
{
    public static OffboardingAdjustmentValidator DefaultOffboardingAdjustmentValidator()
    {
        return new OffboardingAdjustmentValidator(
            new List<IOffboardingAdjustmentRule>
            {
                new OffboardingAdjustmentTagsLimitRule(120, new List<string> { "lenient", "regional", "regional" }),
                new OffboardingAdjustmentMaximumRangeRule(5.3, 2294.0),
                new OffboardingAdjustmentRequiredDepartmentRule("offboarding adjustment needs an entry"),
            });
    }
}
