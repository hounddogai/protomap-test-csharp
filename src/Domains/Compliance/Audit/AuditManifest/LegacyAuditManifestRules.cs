namespace Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>A rule a legacy audit manifest must pass. Check returns its problems, or none.</summary>
public interface ILegacyAuditManifestRule
{
    List<string> Check(LegacyAuditManifest item);
}

public sealed record LegacyAuditManifestRequiredTeamRule(string Message) : ILegacyAuditManifestRule
{
    public List<string> Check(LegacyAuditManifest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditManifestDurationMinutesRangeRule(int Min, int Max) : ILegacyAuditManifestRule
{
    public List<string> Check(LegacyAuditManifest item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditManifestRequiredCodeRule(string Message) : ILegacyAuditManifestRule
{
    public List<string> Check(LegacyAuditManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditManifestRequiredNameRule(string Message) : ILegacyAuditManifestRule
{
    public List<string> Check(LegacyAuditManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditManifestAttemptsRangeRule(int Min, int Max) : ILegacyAuditManifestRule
{
    public List<string> Check(LegacyAuditManifest item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyAuditManifestCapacityRangeRule(int Min, int Max) : ILegacyAuditManifestRule
{
    public List<string> Check(LegacyAuditManifest item)
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

/// <summary>Runs every rule of a legacy audit manifest.</summary>
public sealed record LegacyAuditManifestValidator(List<ILegacyAuditManifestRule> Rules)
{
    public List<string> Validate(LegacyAuditManifest item)
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

    public void RequireValid(LegacyAuditManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAuditManifest> items)
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

/// <summary>Functions of legacy audit manifest rules.</summary>
public static class LegacyAuditManifestRules
{
    public static LegacyAuditManifestValidator DefaultLegacyAuditManifestValidator()
    {
        return new LegacyAuditManifestValidator(
            new List<ILegacyAuditManifestRule>
            {
                new LegacyAuditManifestRequiredTeamRule("legacy audit manifest needs this field"),
                new LegacyAuditManifestDurationMinutesRangeRule(32, 64),
                new LegacyAuditManifestRequiredCodeRule("legacy audit manifest needs a value"),
                new LegacyAuditManifestRequiredNameRule("legacy audit manifest needs a value"),
                new LegacyAuditManifestAttemptsRangeRule(64, 255),
                new LegacyAuditManifestCapacityRangeRule(32, 3600),
            });
    }
}
