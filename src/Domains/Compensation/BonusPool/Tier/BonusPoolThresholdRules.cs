namespace Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>A rule a bonus pool threshold must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolThresholdRule
{
    List<string> Check(BonusPoolThreshold item);
}

public sealed record BonusPoolThresholdRequiredLabelRule(string Message) : IBonusPoolThresholdRule
{
    public List<string> Check(BonusPoolThreshold item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolThresholdRequiredDescriptionRule(string Message) : IBonusPoolThresholdRule
{
    public List<string> Check(BonusPoolThreshold item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolThresholdRequiredCodeRule(string Message) : IBonusPoolThresholdRule
{
    public List<string> Check(BonusPoolThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool threshold.</summary>
public sealed record BonusPoolThresholdValidator(List<IBonusPoolThresholdRule> Rules)
{
    public List<string> Validate(BonusPoolThreshold item)
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

    public void RequireValid(BonusPoolThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolThreshold> items)
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

/// <summary>Functions of bonus pool threshold rules.</summary>
public static class BonusPoolThresholdRules
{
    public static BonusPoolThresholdValidator DefaultBonusPoolThresholdValidator()
    {
        return new BonusPoolThresholdValidator(
            new List<IBonusPoolThresholdRule>
            {
                new BonusPoolThresholdRequiredLabelRule("bonus pool threshold needs an entry"),
                new BonusPoolThresholdRequiredDescriptionRule("bonus pool threshold needs a value"),
                new BonusPoolThresholdRequiredCodeRule("bonus pool threshold needs this field"),
            });
    }
}
