namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>A rule a merit cycle snapshot must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleSnapshotRule
{
    List<string> Check(MeritCycleSnapshot item);
}

public sealed record MeritCycleSnapshotVersionRangeRule(int Min, int Max) : IMeritCycleSnapshotRule
{
    public List<string> Check(MeritCycleSnapshot item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleSnapshotCodeLengthRule(int MaxLength) : IMeritCycleSnapshotRule
{
    public List<string> Check(MeritCycleSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSnapshotCodePrefixRule(string Prefix) : IMeritCycleSnapshotRule
{
    public List<string> Check(MeritCycleSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle snapshot.</summary>
public sealed record MeritCycleSnapshotValidator(List<IMeritCycleSnapshotRule> Rules)
{
    public List<string> Validate(MeritCycleSnapshot item)
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

    public void RequireValid(MeritCycleSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleSnapshot> items)
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

/// <summary>Functions of merit cycle snapshot rules.</summary>
public static class MeritCycleSnapshotRules
{
    public static MeritCycleSnapshotValidator DefaultMeritCycleSnapshotValidator()
    {
        return new MeritCycleSnapshotValidator(
            new List<IMeritCycleSnapshotRule>
            {
                new MeritCycleSnapshotVersionRangeRule(64, 120),
                new MeritCycleSnapshotCodeLengthRule(32),
                new MeritCycleSnapshotCodePrefixRule("X"),
            });
    }
}
