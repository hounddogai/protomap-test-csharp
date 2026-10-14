namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>A rule a bonus pool override must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolOverrideRule
{
    List<string> Check(BonusPoolOverride item);
}

public sealed record BonusPoolOverrideCodeLengthRule(int MaxLength) : IBonusPoolOverrideRule
{
    public List<string> Check(BonusPoolOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolOverrideRequiredCategoryRule(string Message) : IBonusPoolOverrideRule
{
    public List<string> Check(BonusPoolOverride item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolOverrideRequiredCodeRule(string Message) : IBonusPoolOverrideRule
{
    public List<string> Check(BonusPoolOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolOverrideDurationMinutesRangeRule(int Min, int Max) : IBonusPoolOverrideRule
{
    public List<string> Check(BonusPoolOverride item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolOverrideRevisionRangeRule(int Min, int Max) : IBonusPoolOverrideRule
{
    public List<string> Check(BonusPoolOverride item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a bonus pool override.</summary>
public sealed record BonusPoolOverrideValidator(List<IBonusPoolOverrideRule> Rules)
{
    public List<string> Validate(BonusPoolOverride item)
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

    public void RequireValid(BonusPoolOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolOverride> items)
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

/// <summary>Functions of bonus pool override rules.</summary>
public static class BonusPoolOverrideRules
{
    public static BonusPoolOverrideValidator DefaultBonusPoolOverrideValidator()
    {
        return new BonusPoolOverrideValidator(
            new List<IBonusPoolOverrideRule>
            {
                new BonusPoolOverrideCodeLengthRule(16),
                new BonusPoolOverrideRequiredCategoryRule("bonus pool override needs a value"),
                new BonusPoolOverrideRequiredCodeRule("bonus pool override needs an entry"),
                new BonusPoolOverrideDurationMinutesRangeRule(1000, 3600),
                new BonusPoolOverrideRevisionRangeRule(16, 120),
            });
    }
}
