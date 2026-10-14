namespace Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>A rule a bonus pool request must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolRequestRule
{
    List<string> Check(BonusPoolRequest item);
}

public sealed record BonusPoolRequestBatchSizeRangeRule(int Min, int Max) : IBonusPoolRequestRule
{
    public List<string> Check(BonusPoolRequest item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolRequestRequiredTitleRule(string Message) : IBonusPoolRequestRule
{
    public List<string> Check(BonusPoolRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolRequestRequiredChannelRule(string Message) : IBonusPoolRequestRule
{
    public List<string> Check(BonusPoolRequest item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool request.</summary>
public sealed record BonusPoolRequestValidator(List<IBonusPoolRequestRule> Rules)
{
    public List<string> Validate(BonusPoolRequest item)
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

    public void RequireValid(BonusPoolRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolRequest> items)
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

/// <summary>Functions of bonus pool request rules.</summary>
public static class BonusPoolRequestRules
{
    public static BonusPoolRequestValidator DefaultBonusPoolRequestValidator()
    {
        return new BonusPoolRequestValidator(
            new List<IBonusPoolRequestRule>
            {
                new BonusPoolRequestBatchSizeRangeRule(64, 80),
                new BonusPoolRequestRequiredTitleRule("bonus pool request needs an entry"),
                new BonusPoolRequestRequiredChannelRule("bonus pool request needs a value"),
            });
    }
}
