namespace Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>A rule a occupancy digest must pass. Check returns its problems, or none.</summary>
public interface IOccupancyDigestRule
{
    List<string> Check(OccupancyDigest item);
}

public sealed record OccupancyDigestEndsAtRangeRule(int Min, int Max) : IOccupancyDigestRule
{
    public List<string> Check(OccupancyDigest item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyDigestCodeLengthRule(int MaxLength) : IOccupancyDigestRule
{
    public List<string> Check(OccupancyDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyDigestRequiredCodeRule(string Message) : IOccupancyDigestRule
{
    public List<string> Check(OccupancyDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyDigestCodePrefixRule(string Prefix) : IOccupancyDigestRule
{
    public List<string> Check(OccupancyDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyDigestCountryLengthRule(int MaxLength) : IOccupancyDigestRule
{
    public List<string> Check(OccupancyDigest item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyDigestDurationMinutesRangeRule(int Min, int Max) : IOccupancyDigestRule
{
    public List<string> Check(OccupancyDigest item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a occupancy digest.</summary>
public sealed record OccupancyDigestValidator(List<IOccupancyDigestRule> Rules)
{
    public List<string> Validate(OccupancyDigest item)
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

    public void RequireValid(OccupancyDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyDigest> items)
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

/// <summary>Functions of occupancy digest rules.</summary>
public static class OccupancyDigestRules
{
    public static OccupancyDigestValidator DefaultOccupancyDigestValidator()
    {
        return new OccupancyDigestValidator(
            new List<IOccupancyDigestRule>
            {
                new OccupancyDigestEndsAtRangeRule(64, 255),
                new OccupancyDigestCodeLengthRule(16),
                new OccupancyDigestRequiredCodeRule("occupancy digest needs an entry"),
                new OccupancyDigestCodePrefixRule("OPS"),
                new OccupancyDigestCountryLengthRule(80),
                new OccupancyDigestDurationMinutesRangeRule(32, 255),
            });
    }
}
