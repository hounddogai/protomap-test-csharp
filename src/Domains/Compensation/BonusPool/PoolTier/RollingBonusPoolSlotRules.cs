namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>A rule a rolling bonus pool slot must pass. Check returns its problems, or none.</summary>
public interface IRollingBonusPoolSlotRule
{
    List<string> Check(RollingBonusPoolSlot item);
}

public sealed record RollingBonusPoolSlotCodePrefixRule(string Prefix) : IRollingBonusPoolSlotRule
{
    public List<string> Check(RollingBonusPoolSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolSlotCodeLengthRule(int MaxLength) : IRollingBonusPoolSlotRule
{
    public List<string> Check(RollingBonusPoolSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolSlotNameLengthRule(int MaxLength) : IRollingBonusPoolSlotRule
{
    public List<string> Check(RollingBonusPoolSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolSlotRequiredSourceRule(string Message) : IRollingBonusPoolSlotRule
{
    public List<string> Check(RollingBonusPoolSlot item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolSlotLeaveDaysRangeRule(int Min, int Max) : IRollingBonusPoolSlotRule
{
    public List<string> Check(RollingBonusPoolSlot item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolSlotRetriesRangeRule(int Min, int Max) : IRollingBonusPoolSlotRule
{
    public List<string> Check(RollingBonusPoolSlot item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling bonus pool slot.</summary>
public sealed record RollingBonusPoolSlotValidator(List<IRollingBonusPoolSlotRule> Rules)
{
    public List<string> Validate(RollingBonusPoolSlot item)
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

    public void RequireValid(RollingBonusPoolSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBonusPoolSlot> items)
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

/// <summary>Functions of rolling bonus pool slot rules.</summary>
public static class RollingBonusPoolSlotRules
{
    public static RollingBonusPoolSlotValidator DefaultRollingBonusPoolSlotValidator()
    {
        return new RollingBonusPoolSlotValidator(
            new List<IRollingBonusPoolSlotRule>
            {
                new RollingBonusPoolSlotCodePrefixRule("OPS"),
                new RollingBonusPoolSlotCodeLengthRule(32),
                new RollingBonusPoolSlotNameLengthRule(86400),
                new RollingBonusPoolSlotRequiredSourceRule("rolling bonus pool slot needs a value"),
                new RollingBonusPoolSlotLeaveDaysRangeRule(80, 120),
                new RollingBonusPoolSlotRetriesRangeRule(1000, 86400),
            });
    }
}
