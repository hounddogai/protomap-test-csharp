namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>A rule a fallback skill profile snapshot must pass. Check returns its problems, or none.</summary>
public interface IFallbackSkillProfileSnapshotRule
{
    List<string> Check(FallbackSkillProfileSnapshot item);
}

public sealed record FallbackSkillProfileSnapshotCapacityRangeRule(int Min, int Max) : IFallbackSkillProfileSnapshotRule
{
    public List<string> Check(FallbackSkillProfileSnapshot item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackSkillProfileSnapshotArchivedGuardRule(int Limit) : IFallbackSkillProfileSnapshotRule
{
    public List<string> Check(FallbackSkillProfileSnapshot item)
    {
        if (item.Archived && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSkillProfileSnapshotCodePrefixRule(string Prefix) : IFallbackSkillProfileSnapshotRule
{
    public List<string> Check(FallbackSkillProfileSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback skill profile snapshot.</summary>
public sealed record FallbackSkillProfileSnapshotValidator(List<IFallbackSkillProfileSnapshotRule> Rules)
{
    public List<string> Validate(FallbackSkillProfileSnapshot item)
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

    public void RequireValid(FallbackSkillProfileSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSkillProfileSnapshot> items)
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

/// <summary>Functions of fallback skill profile snapshot rules.</summary>
public static class FallbackSkillProfileSnapshotRules
{
    public static FallbackSkillProfileSnapshotValidator DefaultFallbackSkillProfileSnapshotValidator()
    {
        return new FallbackSkillProfileSnapshotValidator(
            new List<IFallbackSkillProfileSnapshotRule>
            {
                new FallbackSkillProfileSnapshotCapacityRangeRule(16, 1000),
                new FallbackSkillProfileSnapshotArchivedGuardRule(32),
                new FallbackSkillProfileSnapshotCodePrefixRule("ACM"),
            });
    }
}
