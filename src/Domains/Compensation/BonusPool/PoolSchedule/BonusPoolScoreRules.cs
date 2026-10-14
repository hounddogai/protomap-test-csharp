namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

/// <summary>A rule a bonus pool score must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolScoreRule
{
    List<string> Check(BonusPoolScore item);
}

public sealed record BonusPoolScoreCodeLengthRule(int MaxLength) : IBonusPoolScoreRule
{
    public List<string> Check(BonusPoolScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolScoreSequenceRangeRule(int Min, int Max) : IBonusPoolScoreRule
{
    public List<string> Check(BonusPoolScore item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolScoreRequiredCodeRule(string Message) : IBonusPoolScoreRule
{
    public List<string> Check(BonusPoolScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolScoreTitleLengthRule(int MaxLength) : IBonusPoolScoreRule
{
    public List<string> Check(BonusPoolScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolScoreCodePrefixRule(string Prefix) : IBonusPoolScoreRule
{
    public List<string> Check(BonusPoolScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool score.</summary>
public sealed record BonusPoolScoreValidator(List<IBonusPoolScoreRule> Rules)
{
    public List<string> Validate(BonusPoolScore item)
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

    public void RequireValid(BonusPoolScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolScore> items)
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

/// <summary>Functions of bonus pool score rules.</summary>
public static class BonusPoolScoreRules
{
    public static BonusPoolScoreValidator DefaultBonusPoolScoreValidator()
    {
        return new BonusPoolScoreValidator(
            new List<IBonusPoolScoreRule>
            {
                new BonusPoolScoreCodeLengthRule(64),
                new BonusPoolScoreSequenceRangeRule(255, 1000),
                new BonusPoolScoreRequiredCodeRule("bonus pool score needs this field"),
                new BonusPoolScoreTitleLengthRule(64),
                new BonusPoolScoreCodePrefixRule("Q"),
            });
    }
}
