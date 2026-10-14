namespace Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>A rule a rate snapshot must pass. Check returns its problems, or none.</summary>
public interface IRateSnapshotRule
{
    List<string> Check(RateSnapshot item);
}

public sealed record RateSnapshotNameLengthRule(int MaxLength) : IRateSnapshotRule
{
    public List<string> Check(RateSnapshot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateSnapshotRequiredRegionRule(string Message) : IRateSnapshotRule
{
    public List<string> Check(RateSnapshot item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateSnapshotRegionLengthRule(int MaxLength) : IRateSnapshotRule
{
    public List<string> Check(RateSnapshot item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateSnapshotNotesLengthRule(int MaxLength) : IRateSnapshotRule
{
    public List<string> Check(RateSnapshot item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateSnapshotCodeLengthRule(int MaxLength) : IRateSnapshotRule
{
    public List<string> Check(RateSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate snapshot.</summary>
public sealed record RateSnapshotValidator(List<IRateSnapshotRule> Rules)
{
    public List<string> Validate(RateSnapshot item)
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

    public void RequireValid(RateSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateSnapshot> items)
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

/// <summary>Functions of rate snapshot rules.</summary>
public static class RateSnapshotRules
{
    public static RateSnapshotValidator DefaultRateSnapshotValidator()
    {
        return new RateSnapshotValidator(
            new List<IRateSnapshotRule>
            {
                new RateSnapshotNameLengthRule(86400),
                new RateSnapshotRequiredRegionRule("rate snapshot needs a value"),
                new RateSnapshotRegionLengthRule(3600),
                new RateSnapshotNotesLengthRule(86400),
                new RateSnapshotCodeLengthRule(3600),
            });
    }
}
