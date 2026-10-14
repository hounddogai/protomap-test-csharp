namespace Acme.Workforce.Talent.Mobility.Window;

/// <summary>A rule a mobility snapshot must pass. Check returns its problems, or none.</summary>
public interface IMobilitySnapshotRule
{
    List<string> Check(MobilitySnapshot item);
}

public sealed record MobilitySnapshotCodePrefixRule(string Prefix) : IMobilitySnapshotRule
{
    public List<string> Check(MobilitySnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilitySnapshotRevisionRangeRule(int Min, int Max) : IMobilitySnapshotRule
{
    public List<string> Check(MobilitySnapshot item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilitySnapshotRequiredCodeRule(string Message) : IMobilitySnapshotRule
{
    public List<string> Check(MobilitySnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilitySnapshotNameLengthRule(int MaxLength) : IMobilitySnapshotRule
{
    public List<string> Check(MobilitySnapshot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilitySnapshotCreatedAtRangeRule(int Min, int Max) : IMobilitySnapshotRule
{
    public List<string> Check(MobilitySnapshot item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MobilitySnapshotUnitLengthRule(int MaxLength) : IMobilitySnapshotRule
{
    public List<string> Check(MobilitySnapshot item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility snapshot.</summary>
public sealed record MobilitySnapshotValidator(List<IMobilitySnapshotRule> Rules)
{
    public List<string> Validate(MobilitySnapshot item)
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

    public void RequireValid(MobilitySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilitySnapshot> items)
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

/// <summary>Functions of mobility snapshot rules.</summary>
public static class MobilitySnapshotRules
{
    public static MobilitySnapshotValidator DefaultMobilitySnapshotValidator()
    {
        return new MobilitySnapshotValidator(
            new List<IMobilitySnapshotRule>
            {
                new MobilitySnapshotCodePrefixRule("REF"),
                new MobilitySnapshotRevisionRangeRule(32, 1000),
                new MobilitySnapshotRequiredCodeRule("mobility snapshot needs this field"),
                new MobilitySnapshotNameLengthRule(80),
                new MobilitySnapshotCreatedAtRangeRule(16, 1000),
                new MobilitySnapshotUnitLengthRule(86400),
            });
    }
}
