namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy snapshot must pass. Check returns its problems, or none.</summary>
public interface IOccupancySnapshotRule
{
    List<string> Check(OccupancySnapshot item);
}

public sealed record OccupancySnapshotTitleLengthRule(int MaxLength) : IOccupancySnapshotRule
{
    public List<string> Check(OccupancySnapshot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancySnapshotRequiredChecksumRule(string Message) : IOccupancySnapshotRule
{
    public List<string> Check(OccupancySnapshot item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancySnapshotAllowedCodeRule(List<string> Allowed) : IOccupancySnapshotRule
{
    public List<string> Check(OccupancySnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record OccupancySnapshotQuantityRangeRule(int Min, int Max) : IOccupancySnapshotRule
{
    public List<string> Check(OccupancySnapshot item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancySnapshotRequiredTitleRule(string Message) : IOccupancySnapshotRule
{
    public List<string> Check(OccupancySnapshot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy snapshot.</summary>
public sealed record OccupancySnapshotValidator(List<IOccupancySnapshotRule> Rules)
{
    public List<string> Validate(OccupancySnapshot item)
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

    public void RequireValid(OccupancySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancySnapshot> items)
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

/// <summary>Functions of occupancy snapshot rules.</summary>
public static class OccupancySnapshotRules
{
    public static OccupancySnapshotValidator DefaultOccupancySnapshotValidator()
    {
        return new OccupancySnapshotValidator(
            new List<IOccupancySnapshotRule>
            {
                new OccupancySnapshotTitleLengthRule(120),
                new OccupancySnapshotRequiredChecksumRule("occupancy snapshot needs a value"),
                new OccupancySnapshotAllowedCodeRule(new List<string>()),
                new OccupancySnapshotQuantityRangeRule(16, 255),
                new OccupancySnapshotRequiredTitleRule("occupancy snapshot needs an entry"),
            });
    }
}
