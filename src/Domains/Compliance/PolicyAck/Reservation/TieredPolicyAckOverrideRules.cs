namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>A rule a tiered policy ack override must pass. Check returns its problems, or none.</summary>
public interface ITieredPolicyAckOverrideRule
{
    List<string> Check(TieredPolicyAckOverride item);
}

public sealed record TieredPolicyAckOverrideDurationMinutesRangeRule(int Min, int Max) : ITieredPolicyAckOverrideRule
{
    public List<string> Check(TieredPolicyAckOverride item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckOverrideAttemptsRangeRule(int Min, int Max) : ITieredPolicyAckOverrideRule
{
    public List<string> Check(TieredPolicyAckOverride item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckOverrideHoursRangeRule(double Min, double Max) : ITieredPolicyAckOverrideRule
{
    public List<string> Check(TieredPolicyAckOverride item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredPolicyAckOverridePhoneLengthRule(int MaxLength) : ITieredPolicyAckOverrideRule
{
    public List<string> Check(TieredPolicyAckOverride item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckOverrideRequiredCodeRule(string Message) : ITieredPolicyAckOverrideRule
{
    public List<string> Check(TieredPolicyAckOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered policy ack override.</summary>
public sealed record TieredPolicyAckOverrideValidator(List<ITieredPolicyAckOverrideRule> Rules)
{
    public List<string> Validate(TieredPolicyAckOverride item)
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

    public void RequireValid(TieredPolicyAckOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPolicyAckOverride> items)
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

/// <summary>Functions of tiered policy ack override rules.</summary>
public static class TieredPolicyAckOverrideRules
{
    public static TieredPolicyAckOverrideValidator DefaultTieredPolicyAckOverrideValidator()
    {
        return new TieredPolicyAckOverrideValidator(
            new List<ITieredPolicyAckOverrideRule>
            {
                new TieredPolicyAckOverrideDurationMinutesRangeRule(16, 86400),
                new TieredPolicyAckOverrideAttemptsRangeRule(3600, 86400),
                new TieredPolicyAckOverrideHoursRangeRule(4.6, 313.7),
                new TieredPolicyAckOverridePhoneLengthRule(120),
                new TieredPolicyAckOverrideRequiredCodeRule("tiered policy ack override needs this field"),
            });
    }
}
