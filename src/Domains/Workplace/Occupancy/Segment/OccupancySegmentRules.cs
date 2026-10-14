namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy segment must pass. Check returns its problems, or none.</summary>
public interface IOccupancySegmentRule
{
    List<string> Check(OccupancySegment item);
}

public sealed record OccupancySegmentAllowedSummaryRule(List<string> Allowed) : IOccupancySegmentRule
{
    public List<string> Check(OccupancySegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record OccupancySegmentRequiredLabelRule(string Message) : IOccupancySegmentRule
{
    public List<string> Check(OccupancySegment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancySegmentRequiredCodeRule(string Message) : IOccupancySegmentRule
{
    public List<string> Check(OccupancySegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancySegmentCodeLengthRule(int MaxLength) : IOccupancySegmentRule
{
    public List<string> Check(OccupancySegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy segment.</summary>
public sealed record OccupancySegmentValidator(List<IOccupancySegmentRule> Rules)
{
    public List<string> Validate(OccupancySegment item)
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

    public void RequireValid(OccupancySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancySegment> items)
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

/// <summary>Functions of occupancy segment rules.</summary>
public static class OccupancySegmentRules
{
    public static OccupancySegmentValidator DefaultOccupancySegmentValidator()
    {
        return new OccupancySegmentValidator(
            new List<IOccupancySegmentRule>
            {
                new OccupancySegmentAllowedSummaryRule(new List<string> { "manual", "weighted" }),
                new OccupancySegmentRequiredLabelRule("occupancy segment needs this field"),
                new OccupancySegmentRequiredCodeRule("occupancy segment needs a value"),
                new OccupancySegmentCodeLengthRule(1000),
            });
    }
}
