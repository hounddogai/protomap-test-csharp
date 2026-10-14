namespace Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>A rule a position slot must pass. Check returns its problems, or none.</summary>
public interface IPositionSlotRule
{
    List<string> Check(PositionSlot item);
}

public sealed record PositionSlotTagsLimitRule(int MaxCount, List<string> Banned) : IPositionSlotRule
{
    public List<string> Check(PositionSlot item)
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

public sealed record PositionSlotNameLengthRule(int MaxLength) : IPositionSlotRule
{
    public List<string> Check(PositionSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSlotAttemptsRangeRule(int Min, int Max) : IPositionSlotRule
{
    public List<string> Check(PositionSlot item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSlotCodePrefixRule(string Prefix) : IPositionSlotRule
{
    public List<string> Check(PositionSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSlotChannelLengthRule(int MaxLength) : IPositionSlotRule
{
    public List<string> Check(PositionSlot item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position slot.</summary>
public sealed record PositionSlotValidator(List<IPositionSlotRule> Rules)
{
    public List<string> Validate(PositionSlot item)
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

    public void RequireValid(PositionSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionSlot> items)
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

/// <summary>Functions of position slot rules.</summary>
public static class PositionSlotRules
{
    public static PositionSlotValidator DefaultPositionSlotValidator()
    {
        return new PositionSlotValidator(
            new List<IPositionSlotRule>
            {
                new PositionSlotTagsLimitRule(32, new List<string>()),
                new PositionSlotNameLengthRule(16),
                new PositionSlotAttemptsRangeRule(64, 120),
                new PositionSlotCodePrefixRule("REF"),
                new PositionSlotChannelLengthRule(86400),
            });
    }
}
