namespace Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>A rule a bonus pool checkpoint must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolCheckpointRule
{
    List<string> Check(BonusPoolCheckpoint item);
}

public sealed record BonusPoolCheckpointRequiredCodeRule(string Message) : IBonusPoolCheckpointRule
{
    public List<string> Check(BonusPoolCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolCheckpointRequiredReviewerRule(string Message) : IBonusPoolCheckpointRule
{
    public List<string> Check(BonusPoolCheckpoint item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolCheckpointAmountRangeRule(double Min, double Max) : IBonusPoolCheckpointRule
{
    public List<string> Check(BonusPoolCheckpoint item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool checkpoint.</summary>
public sealed record BonusPoolCheckpointValidator(List<IBonusPoolCheckpointRule> Rules)
{
    public List<string> Validate(BonusPoolCheckpoint item)
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

    public void RequireValid(BonusPoolCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolCheckpoint> items)
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

/// <summary>Functions of bonus pool checkpoint rules.</summary>
public static class BonusPoolCheckpointRules
{
    public static BonusPoolCheckpointValidator DefaultBonusPoolCheckpointValidator()
    {
        return new BonusPoolCheckpointValidator(
            new List<IBonusPoolCheckpointRule>
            {
                new BonusPoolCheckpointRequiredCodeRule("bonus pool checkpoint needs this field"),
                new BonusPoolCheckpointRequiredReviewerRule("bonus pool checkpoint needs an entry"),
                new BonusPoolCheckpointAmountRangeRule(9.8, 4337.5),
            });
    }
}
