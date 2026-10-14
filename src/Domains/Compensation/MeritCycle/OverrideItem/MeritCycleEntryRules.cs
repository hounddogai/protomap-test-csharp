namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>A rule a merit cycle entry must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleEntryRule
{
    List<string> Check(MeritCycleEntry item);
}

public sealed record MeritCycleEntryRequiredBadgeRule(string Message) : IMeritCycleEntryRule
{
    public List<string> Check(MeritCycleEntry item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleEntryRequiredManagerIdRule(string Message) : IMeritCycleEntryRule
{
    public List<string> Check(MeritCycleEntry item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleEntryRatioRangeRule(double Min, double Max) : IMeritCycleEntryRule
{
    public List<string> Check(MeritCycleEntry item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleEntryBadgeLengthRule(int MaxLength) : IMeritCycleEntryRule
{
    public List<string> Check(MeritCycleEntry item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleEntryRevisionRangeRule(int Min, int Max) : IMeritCycleEntryRule
{
    public List<string> Check(MeritCycleEntry item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleEntryCodePrefixRule(string Prefix) : IMeritCycleEntryRule
{
    public List<string> Check(MeritCycleEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle entry.</summary>
public sealed record MeritCycleEntryValidator(List<IMeritCycleEntryRule> Rules)
{
    public List<string> Validate(MeritCycleEntry item)
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

    public void RequireValid(MeritCycleEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleEntry> items)
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

/// <summary>Functions of merit cycle entry rules.</summary>
public static class MeritCycleEntryRules
{
    public static MeritCycleEntryValidator DefaultMeritCycleEntryValidator()
    {
        return new MeritCycleEntryValidator(
            new List<IMeritCycleEntryRule>
            {
                new MeritCycleEntryRequiredBadgeRule("merit cycle entry needs a value"),
                new MeritCycleEntryRequiredManagerIdRule("merit cycle entry needs this field"),
                new MeritCycleEntryRatioRangeRule(2.4, 58.8),
                new MeritCycleEntryBadgeLengthRule(80),
                new MeritCycleEntryRevisionRangeRule(80, 120),
                new MeritCycleEntryCodePrefixRule("X"),
            });
    }
}
