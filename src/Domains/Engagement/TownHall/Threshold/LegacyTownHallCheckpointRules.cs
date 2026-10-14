namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy town hall checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILegacyTownHallCheckpointRule
{
    List<string> Check(LegacyTownHallCheckpoint item);
}

public sealed record LegacyTownHallCheckpointFteRangeRule(double Min, double Max) : ILegacyTownHallCheckpointRule
{
    public List<string> Check(LegacyTownHallCheckpoint item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyTownHallCheckpointArchivedGuardRule(double Limit) : ILegacyTownHallCheckpointRule
{
    public List<string> Check(LegacyTownHallCheckpoint item)
    {
        if (item.Archived && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallCheckpointRequiredRegionRule(string Message) : ILegacyTownHallCheckpointRule
{
    public List<string> Check(LegacyTownHallCheckpoint item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallCheckpointCodeLengthRule(int MaxLength) : ILegacyTownHallCheckpointRule
{
    public List<string> Check(LegacyTownHallCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallCheckpointAllowedLabelRule(List<string> Allowed) : ILegacyTownHallCheckpointRule
{
    public List<string> Check(LegacyTownHallCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallCheckpointRequiredLabelRule(string Message) : ILegacyTownHallCheckpointRule
{
    public List<string> Check(LegacyTownHallCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy town hall checkpoint.</summary>
public sealed record LegacyTownHallCheckpointValidator(List<ILegacyTownHallCheckpointRule> Rules)
{
    public List<string> Validate(LegacyTownHallCheckpoint item)
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

    public void RequireValid(LegacyTownHallCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTownHallCheckpoint> items)
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

/// <summary>Functions of legacy town hall checkpoint rules.</summary>
public static class LegacyTownHallCheckpointRules
{
    public static LegacyTownHallCheckpointValidator DefaultLegacyTownHallCheckpointValidator()
    {
        return new LegacyTownHallCheckpointValidator(
            new List<ILegacyTownHallCheckpointRule>
            {
                new LegacyTownHallCheckpointFteRangeRule(0.4, 2056.2),
                new LegacyTownHallCheckpointArchivedGuardRule(8.2),
                new LegacyTownHallCheckpointRequiredRegionRule("legacy town hall checkpoint needs an entry"),
                new LegacyTownHallCheckpointCodeLengthRule(32),
                new LegacyTownHallCheckpointAllowedLabelRule(new List<string> { "fallback" }),
                new LegacyTownHallCheckpointRequiredLabelRule("legacy town hall checkpoint needs a value"),
            });
    }
}
