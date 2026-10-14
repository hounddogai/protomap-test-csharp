namespace Acme.Workforce.Compliance.PolicyAck.Schedule;

/// <summary>A rule a tiered policy ack schedule must pass. Check returns its problems, or none.</summary>
public interface ITieredPolicyAckScheduleRule
{
    List<string> Check(TieredPolicyAckSchedule item);
}

public sealed record TieredPolicyAckScheduleTitleLengthRule(int MaxLength) : ITieredPolicyAckScheduleRule
{
    public List<string> Check(TieredPolicyAckSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckScheduleRequiredCodeRule(string Message) : ITieredPolicyAckScheduleRule
{
    public List<string> Check(TieredPolicyAckSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckScheduleRequiredTitleRule(string Message) : ITieredPolicyAckScheduleRule
{
    public List<string> Check(TieredPolicyAckSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered policy ack schedule.</summary>
public sealed record TieredPolicyAckScheduleValidator(List<ITieredPolicyAckScheduleRule> Rules)
{
    public List<string> Validate(TieredPolicyAckSchedule item)
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

    public void RequireValid(TieredPolicyAckSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPolicyAckSchedule> items)
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

/// <summary>Functions of tiered policy ack schedule rules.</summary>
public static class TieredPolicyAckScheduleRules
{
    public static TieredPolicyAckScheduleValidator DefaultTieredPolicyAckScheduleValidator()
    {
        return new TieredPolicyAckScheduleValidator(
            new List<ITieredPolicyAckScheduleRule>
            {
                new TieredPolicyAckScheduleTitleLengthRule(120),
                new TieredPolicyAckScheduleRequiredCodeRule("tiered policy ack schedule needs a value"),
                new TieredPolicyAckScheduleRequiredTitleRule("tiered policy ack schedule needs this field"),
            });
    }
}
