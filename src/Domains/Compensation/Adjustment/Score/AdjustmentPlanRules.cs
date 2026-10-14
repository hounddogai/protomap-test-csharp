namespace Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>A rule a adjustment plan must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentPlanRule
{
    List<string> Check(AdjustmentPlan item);
}

public sealed record AdjustmentPlanRequiredSkillRule(string Message) : IAdjustmentPlanRule
{
    public List<string> Check(AdjustmentPlan item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentPlanCodePrefixRule(string Prefix) : IAdjustmentPlanRule
{
    public List<string> Check(AdjustmentPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentPlanJobCodeLengthRule(int MaxLength) : IAdjustmentPlanRule
{
    public List<string> Check(AdjustmentPlan item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentPlanCapacityRangeRule(int Min, int Max) : IAdjustmentPlanRule
{
    public List<string> Check(AdjustmentPlan item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentPlanLabelLengthRule(int MaxLength) : IAdjustmentPlanRule
{
    public List<string> Check(AdjustmentPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment plan.</summary>
public sealed record AdjustmentPlanValidator(List<IAdjustmentPlanRule> Rules)
{
    public List<string> Validate(AdjustmentPlan item)
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

    public void RequireValid(AdjustmentPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentPlan> items)
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

/// <summary>Functions of adjustment plan rules.</summary>
public static class AdjustmentPlanRules
{
    public static AdjustmentPlanValidator DefaultAdjustmentPlanValidator()
    {
        return new AdjustmentPlanValidator(
            new List<IAdjustmentPlanRule>
            {
                new AdjustmentPlanRequiredSkillRule("adjustment plan needs an entry"),
                new AdjustmentPlanCodePrefixRule("ACM"),
                new AdjustmentPlanJobCodeLengthRule(86400),
                new AdjustmentPlanCapacityRangeRule(32, 3600),
                new AdjustmentPlanLabelLengthRule(3600),
            });
    }
}
