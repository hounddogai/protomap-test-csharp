namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>A rule a seasonal policy ack entry must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPolicyAckEntryRule
{
    List<string> Check(SeasonalPolicyAckEntry item);
}

public sealed record SeasonalPolicyAckEntryRequiredCodeRule(string Message) : ISeasonalPolicyAckEntryRule
{
    public List<string> Check(SeasonalPolicyAckEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPolicyAckEntryCodeLengthRule(int MaxLength) : ISeasonalPolicyAckEntryRule
{
    public List<string> Check(SeasonalPolicyAckEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPolicyAckEntryCostRangeRule(double Min, double Max) : ISeasonalPolicyAckEntryRule
{
    public List<string> Check(SeasonalPolicyAckEntry item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal policy ack entry.</summary>
public sealed record SeasonalPolicyAckEntryValidator(List<ISeasonalPolicyAckEntryRule> Rules)
{
    public List<string> Validate(SeasonalPolicyAckEntry item)
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

    public void RequireValid(SeasonalPolicyAckEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPolicyAckEntry> items)
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

/// <summary>Functions of seasonal policy ack entry rules.</summary>
public static class SeasonalPolicyAckEntryRules
{
    public static SeasonalPolicyAckEntryValidator DefaultSeasonalPolicyAckEntryValidator()
    {
        return new SeasonalPolicyAckEntryValidator(
            new List<ISeasonalPolicyAckEntryRule>
            {
                new SeasonalPolicyAckEntryRequiredCodeRule("seasonal policy ack entry needs this field"),
                new SeasonalPolicyAckEntryCodeLengthRule(86400),
                new SeasonalPolicyAckEntryCostRangeRule(3.9, 573.2),
            });
    }
}
