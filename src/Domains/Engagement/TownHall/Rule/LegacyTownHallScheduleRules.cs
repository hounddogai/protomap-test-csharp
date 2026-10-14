namespace Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>A rule a legacy town hall schedule must pass. Check returns its problems, or none.</summary>
public interface ILegacyTownHallScheduleRule
{
    List<string> Check(LegacyTownHallSchedule item);
}

public sealed record LegacyTownHallScheduleSourceLengthRule(int MaxLength) : ILegacyTownHallScheduleRule
{
    public List<string> Check(LegacyTownHallSchedule item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallScheduleCodeLengthRule(int MaxLength) : ILegacyTownHallScheduleRule
{
    public List<string> Check(LegacyTownHallSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallScheduleRequiredTitleRule(string Message) : ILegacyTownHallScheduleRule
{
    public List<string> Check(LegacyTownHallSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallScheduleCodePrefixRule(string Prefix) : ILegacyTownHallScheduleRule
{
    public List<string> Check(LegacyTownHallSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallScheduleCeilingRangeRule(double Min, double Max) : ILegacyTownHallScheduleRule
{
    public List<string> Check(LegacyTownHallSchedule item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy town hall schedule.</summary>
public sealed record LegacyTownHallScheduleValidator(List<ILegacyTownHallScheduleRule> Rules)
{
    public List<string> Validate(LegacyTownHallSchedule item)
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

    public void RequireValid(LegacyTownHallSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTownHallSchedule> items)
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

/// <summary>Functions of legacy town hall schedule rules.</summary>
public static class LegacyTownHallScheduleRules
{
    public static LegacyTownHallScheduleValidator DefaultLegacyTownHallScheduleValidator()
    {
        return new LegacyTownHallScheduleValidator(
            new List<ILegacyTownHallScheduleRule>
            {
                new LegacyTownHallScheduleSourceLengthRule(1000),
                new LegacyTownHallScheduleCodeLengthRule(32),
                new LegacyTownHallScheduleRequiredTitleRule("legacy town hall schedule needs this field"),
                new LegacyTownHallScheduleCodePrefixRule("REF"),
                new LegacyTownHallScheduleCeilingRangeRule(9.2, 3012.5),
            });
    }
}
