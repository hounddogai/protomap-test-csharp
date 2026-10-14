namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched equity grant preference must pass. Check returns its problems, or none.</summary>
public interface IBatchedEquityGrantPreferenceRule
{
    List<string> Check(BatchedEquityGrantPreference item);
}

public sealed record BatchedEquityGrantPreferenceRatingRangeRule(double Min, double Max) : IBatchedEquityGrantPreferenceRule
{
    public List<string> Check(BatchedEquityGrantPreference item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedEquityGrantPreferenceAllowedTeamRule(List<string> Allowed) : IBatchedEquityGrantPreferenceRule
{
    public List<string> Check(BatchedEquityGrantPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record BatchedEquityGrantPreferenceTeamLengthRule(int MaxLength) : IBatchedEquityGrantPreferenceRule
{
    public List<string> Check(BatchedEquityGrantPreference item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedEquityGrantPreferenceBatchSizeRangeRule(int Min, int Max) : IBatchedEquityGrantPreferenceRule
{
    public List<string> Check(BatchedEquityGrantPreference item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedEquityGrantPreferenceRequiredTeamRule(string Message) : IBatchedEquityGrantPreferenceRule
{
    public List<string> Check(BatchedEquityGrantPreference item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched equity grant preference.</summary>
public sealed record BatchedEquityGrantPreferenceValidator(List<IBatchedEquityGrantPreferenceRule> Rules)
{
    public List<string> Validate(BatchedEquityGrantPreference item)
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

    public void RequireValid(BatchedEquityGrantPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedEquityGrantPreference> items)
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

/// <summary>Functions of batched equity grant preference rules.</summary>
public static class BatchedEquityGrantPreferenceRules
{
    public static BatchedEquityGrantPreferenceValidator DefaultBatchedEquityGrantPreferenceValidator()
    {
        return new BatchedEquityGrantPreferenceValidator(
            new List<IBatchedEquityGrantPreferenceRule>
            {
                new BatchedEquityGrantPreferenceRatingRangeRule(3.5, 1615.8),
                new BatchedEquityGrantPreferenceAllowedTeamRule(new List<string> { "pending", "primary", "strict" }),
                new BatchedEquityGrantPreferenceTeamLengthRule(255),
                new BatchedEquityGrantPreferenceBatchSizeRangeRule(3600, 86400),
                new BatchedEquityGrantPreferenceRequiredTeamRule("batched equity grant preference needs an entry"),
            });
    }
}
