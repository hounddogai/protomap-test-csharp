namespace Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>A rule a bonus pool cycle must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolCycleRule
{
    List<string> Check(BonusPoolCycle item);
}

public sealed record BonusPoolCycleManagerIdLengthRule(int MaxLength) : IBonusPoolCycleRule
{
    public List<string> Check(BonusPoolCycle item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolCycleRequiredManagerIdRule(string Message) : IBonusPoolCycleRule
{
    public List<string> Check(BonusPoolCycle item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolCycleCodePrefixRule(string Prefix) : IBonusPoolCycleRule
{
    public List<string> Check(BonusPoolCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolCycleLabelLengthRule(int MaxLength) : IBonusPoolCycleRule
{
    public List<string> Check(BonusPoolCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool cycle.</summary>
public sealed record BonusPoolCycleValidator(List<IBonusPoolCycleRule> Rules)
{
    public List<string> Validate(BonusPoolCycle item)
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

    public void RequireValid(BonusPoolCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolCycle> items)
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

/// <summary>Functions of bonus pool cycle rules.</summary>
public static class BonusPoolCycleRules
{
    public static BonusPoolCycleValidator DefaultBonusPoolCycleValidator()
    {
        return new BonusPoolCycleValidator(
            new List<IBonusPoolCycleRule>
            {
                new BonusPoolCycleManagerIdLengthRule(32),
                new BonusPoolCycleRequiredManagerIdRule("bonus pool cycle needs a value"),
                new BonusPoolCycleCodePrefixRule("REF"),
                new BonusPoolCycleLabelLengthRule(16),
            });
    }
}
