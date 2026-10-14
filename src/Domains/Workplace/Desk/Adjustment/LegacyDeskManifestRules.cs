namespace Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>A rule a legacy desk manifest must pass. Check returns its problems, or none.</summary>
public interface ILegacyDeskManifestRule
{
    List<string> Check(LegacyDeskManifest item);
}

public sealed record LegacyDeskManifestHoursRangeRule(double Min, double Max) : ILegacyDeskManifestRule
{
    public List<string> Check(LegacyDeskManifest item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeskManifestExpiresAtRangeRule(int Min, int Max) : ILegacyDeskManifestRule
{
    public List<string> Check(LegacyDeskManifest item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyDeskManifestRequiredSegmentRule(string Message) : ILegacyDeskManifestRule
{
    public List<string> Check(LegacyDeskManifest item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeskManifestPinnedGuardRule(int Limit) : ILegacyDeskManifestRule
{
    public List<string> Check(LegacyDeskManifest item)
    {
        if (item.Pinned && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeskManifestCountryLengthRule(int MaxLength) : ILegacyDeskManifestRule
{
    public List<string> Check(LegacyDeskManifest item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy desk manifest.</summary>
public sealed record LegacyDeskManifestValidator(List<ILegacyDeskManifestRule> Rules)
{
    public List<string> Validate(LegacyDeskManifest item)
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

    public void RequireValid(LegacyDeskManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyDeskManifest> items)
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

/// <summary>Functions of legacy desk manifest rules.</summary>
public static class LegacyDeskManifestRules
{
    public static LegacyDeskManifestValidator DefaultLegacyDeskManifestValidator()
    {
        return new LegacyDeskManifestValidator(
            new List<ILegacyDeskManifestRule>
            {
                new LegacyDeskManifestHoursRangeRule(0.5, 308.8),
                new LegacyDeskManifestExpiresAtRangeRule(32, 80),
                new LegacyDeskManifestRequiredSegmentRule("legacy desk manifest needs this field"),
                new LegacyDeskManifestPinnedGuardRule(86400),
                new LegacyDeskManifestCountryLengthRule(86400),
            });
    }
}
