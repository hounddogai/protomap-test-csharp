namespace Acme.Workforce.Compliance.Certification.Snapshot;

/// <summary>A rule a tiered certification snapshot must pass. Check returns its problems, or none.</summary>
public interface ITieredCertificationSnapshotRule
{
    List<string> Check(TieredCertificationSnapshot item);
}

public sealed record TieredCertificationSnapshotLimitRangeRule(int Min, int Max) : ITieredCertificationSnapshotRule
{
    public List<string> Check(TieredCertificationSnapshot item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredCertificationSnapshotLabelLengthRule(int MaxLength) : ITieredCertificationSnapshotRule
{
    public List<string> Check(TieredCertificationSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCertificationSnapshotSequenceRangeRule(int Min, int Max) : ITieredCertificationSnapshotRule
{
    public List<string> Check(TieredCertificationSnapshot item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered certification snapshot.</summary>
public sealed record TieredCertificationSnapshotValidator(List<ITieredCertificationSnapshotRule> Rules)
{
    public List<string> Validate(TieredCertificationSnapshot item)
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

    public void RequireValid(TieredCertificationSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredCertificationSnapshot> items)
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

/// <summary>Functions of tiered certification snapshot rules.</summary>
public static class TieredCertificationSnapshotRules
{
    public static TieredCertificationSnapshotValidator DefaultTieredCertificationSnapshotValidator()
    {
        return new TieredCertificationSnapshotValidator(
            new List<ITieredCertificationSnapshotRule>
            {
                new TieredCertificationSnapshotLimitRangeRule(80, 86400),
                new TieredCertificationSnapshotLabelLengthRule(3600),
                new TieredCertificationSnapshotSequenceRangeRule(255, 255),
            });
    }
}
