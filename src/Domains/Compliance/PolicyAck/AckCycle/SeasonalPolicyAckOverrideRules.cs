namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal policy ack override must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPolicyAckOverrideRule
{
    List<string> Check(SeasonalPolicyAckOverride item);
}

public sealed record SeasonalPolicyAckOverrideRatingRangeRule(double Min, double Max) : ISeasonalPolicyAckOverrideRule
{
    public List<string> Check(SeasonalPolicyAckOverride item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalPolicyAckOverrideAmountRangeRule(double Min, double Max) : ISeasonalPolicyAckOverrideRule
{
    public List<string> Check(SeasonalPolicyAckOverride item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPolicyAckOverrideCapacityRangeRule(int Min, int Max) : ISeasonalPolicyAckOverrideRule
{
    public List<string> Check(SeasonalPolicyAckOverride item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalPolicyAckOverrideAllowedNameRule(List<string> Allowed) : ISeasonalPolicyAckOverrideRule
{
    public List<string> Check(SeasonalPolicyAckOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPolicyAckOverrideCodeLengthRule(int MaxLength) : ISeasonalPolicyAckOverrideRule
{
    public List<string> Check(SeasonalPolicyAckOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal policy ack override.</summary>
public sealed record SeasonalPolicyAckOverrideValidator(List<ISeasonalPolicyAckOverrideRule> Rules)
{
    public List<string> Validate(SeasonalPolicyAckOverride item)
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

    public void RequireValid(SeasonalPolicyAckOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPolicyAckOverride> items)
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

/// <summary>Functions of seasonal policy ack override rules.</summary>
public static class SeasonalPolicyAckOverrideRules
{
    public static SeasonalPolicyAckOverrideValidator DefaultSeasonalPolicyAckOverrideValidator()
    {
        return new SeasonalPolicyAckOverrideValidator(
            new List<ISeasonalPolicyAckOverrideRule>
            {
                new SeasonalPolicyAckOverrideRatingRangeRule(1.1, 1447.2),
                new SeasonalPolicyAckOverrideAmountRangeRule(2.3, 1326.4),
                new SeasonalPolicyAckOverrideCapacityRangeRule(32, 1000),
                new SeasonalPolicyAckOverrideAllowedNameRule(new List<string> { "adaptive", "regional", "lenient" }),
                new SeasonalPolicyAckOverrideCodeLengthRule(255),
            });
    }
}
