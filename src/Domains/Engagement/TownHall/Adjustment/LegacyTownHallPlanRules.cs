namespace Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>A rule a legacy town hall plan must pass. Check returns its problems, or none.</summary>
public interface ILegacyTownHallPlanRule
{
    List<string> Check(LegacyTownHallPlan item);
}

public sealed record LegacyTownHallPlanManagerIdLengthRule(int MaxLength) : ILegacyTownHallPlanRule
{
    public List<string> Check(LegacyTownHallPlan item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallPlanRequiredChecksumRule(string Message) : ILegacyTownHallPlanRule
{
    public List<string> Check(LegacyTownHallPlan item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallPlanCodePrefixRule(string Prefix) : ILegacyTownHallPlanRule
{
    public List<string> Check(LegacyTownHallPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallPlanRevisionRangeRule(int Min, int Max) : ILegacyTownHallPlanRule
{
    public List<string> Check(LegacyTownHallPlan item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallPlanArchivedGuardRule(int Limit) : ILegacyTownHallPlanRule
{
    public List<string> Check(LegacyTownHallPlan item)
    {
        if (item.Archived && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy town hall plan.</summary>
public sealed record LegacyTownHallPlanValidator(List<ILegacyTownHallPlanRule> Rules)
{
    public List<string> Validate(LegacyTownHallPlan item)
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

    public void RequireValid(LegacyTownHallPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTownHallPlan> items)
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

/// <summary>Functions of legacy town hall plan rules.</summary>
public static class LegacyTownHallPlanRules
{
    public static LegacyTownHallPlanValidator DefaultLegacyTownHallPlanValidator()
    {
        return new LegacyTownHallPlanValidator(
            new List<ILegacyTownHallPlanRule>
            {
                new LegacyTownHallPlanManagerIdLengthRule(86400),
                new LegacyTownHallPlanRequiredChecksumRule("legacy town hall plan needs this field"),
                new LegacyTownHallPlanCodePrefixRule("ACM"),
                new LegacyTownHallPlanRevisionRangeRule(64, 1000),
                new LegacyTownHallPlanArchivedGuardRule(80),
            });
    }
}
