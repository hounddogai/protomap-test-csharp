namespace Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>A rule a legacy town hall cycle must pass. Check returns its problems, or none.</summary>
public interface ILegacyTownHallCycleRule
{
    List<string> Check(LegacyTownHallCycle item);
}

public sealed record LegacyTownHallCycleRequiredTitleRule(string Message) : ILegacyTownHallCycleRule
{
    public List<string> Check(LegacyTownHallCycle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallCycleQuantityRangeRule(int Min, int Max) : ILegacyTownHallCycleRule
{
    public List<string> Check(LegacyTownHallCycle item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallCycleCodeLengthRule(int MaxLength) : ILegacyTownHallCycleRule
{
    public List<string> Check(LegacyTownHallCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallCycleTagsLimitRule(int MaxCount, List<string> Banned) : ILegacyTownHallCycleRule
{
    public List<string> Check(LegacyTownHallCycle item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record LegacyTownHallCycleTitleLengthRule(int MaxLength) : ILegacyTownHallCycleRule
{
    public List<string> Check(LegacyTownHallCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallCycleRequiredSummaryRule(string Message) : ILegacyTownHallCycleRule
{
    public List<string> Check(LegacyTownHallCycle item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy town hall cycle.</summary>
public sealed record LegacyTownHallCycleValidator(List<ILegacyTownHallCycleRule> Rules)
{
    public List<string> Validate(LegacyTownHallCycle item)
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

    public void RequireValid(LegacyTownHallCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTownHallCycle> items)
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

/// <summary>Functions of legacy town hall cycle rules.</summary>
public static class LegacyTownHallCycleRules
{
    public static LegacyTownHallCycleValidator DefaultLegacyTownHallCycleValidator()
    {
        return new LegacyTownHallCycleValidator(
            new List<ILegacyTownHallCycleRule>
            {
                new LegacyTownHallCycleRequiredTitleRule("legacy town hall cycle needs an entry"),
                new LegacyTownHallCycleQuantityRangeRule(80, 3600),
                new LegacyTownHallCycleCodeLengthRule(120),
                new LegacyTownHallCycleTagsLimitRule(1000, new List<string> { "adaptive" }),
                new LegacyTownHallCycleTitleLengthRule(80),
                new LegacyTownHallCycleRequiredSummaryRule("legacy town hall cycle needs a value"),
            });
    }
}
