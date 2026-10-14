namespace Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>A rule a bonus pool estimate must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolEstimateRule
{
    List<string> Check(BonusPoolEstimate item);
}

public sealed record BonusPoolEstimateRequiredTimezoneRule(string Message) : IBonusPoolEstimateRule
{
    public List<string> Check(BonusPoolEstimate item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolEstimateNameLengthRule(int MaxLength) : IBonusPoolEstimateRule
{
    public List<string> Check(BonusPoolEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolEstimateRequiredCodeRule(string Message) : IBonusPoolEstimateRule
{
    public List<string> Check(BonusPoolEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool estimate.</summary>
public sealed record BonusPoolEstimateValidator(List<IBonusPoolEstimateRule> Rules)
{
    public List<string> Validate(BonusPoolEstimate item)
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

    public void RequireValid(BonusPoolEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolEstimate> items)
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

/// <summary>Functions of bonus pool estimate rules.</summary>
public static class BonusPoolEstimateRules
{
    public static BonusPoolEstimateValidator DefaultBonusPoolEstimateValidator()
    {
        return new BonusPoolEstimateValidator(
            new List<IBonusPoolEstimateRule>
            {
                new BonusPoolEstimateRequiredTimezoneRule("bonus pool estimate needs an entry"),
                new BonusPoolEstimateNameLengthRule(16),
                new BonusPoolEstimateRequiredCodeRule("bonus pool estimate needs this field"),
            });
    }
}
