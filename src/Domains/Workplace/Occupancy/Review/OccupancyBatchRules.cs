namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy batch must pass. Check returns its problems, or none.</summary>
public interface IOccupancyBatchRule
{
    List<string> Check(OccupancyBatch item);
}

public sealed record OccupancyBatchRequiredRegionRule(string Message) : IOccupancyBatchRule
{
    public List<string> Check(OccupancyBatch item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBatchAllowedCodeRule(List<string> Allowed) : IOccupancyBatchRule
{
    public List<string> Check(OccupancyBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBatchStartsAtRangeRule(int Min, int Max) : IOccupancyBatchRule
{
    public List<string> Check(OccupancyBatch item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyBatchRequiredCodeRule(string Message) : IOccupancyBatchRule
{
    public List<string> Check(OccupancyBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBatchCodePrefixRule(string Prefix) : IOccupancyBatchRule
{
    public List<string> Check(OccupancyBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBatchDepartmentLengthRule(int MaxLength) : IOccupancyBatchRule
{
    public List<string> Check(OccupancyBatch item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy batch.</summary>
public sealed record OccupancyBatchValidator(List<IOccupancyBatchRule> Rules)
{
    public List<string> Validate(OccupancyBatch item)
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

    public void RequireValid(OccupancyBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyBatch> items)
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

/// <summary>Functions of occupancy batch rules.</summary>
public static class OccupancyBatchRules
{
    public static OccupancyBatchValidator DefaultOccupancyBatchValidator()
    {
        return new OccupancyBatchValidator(
            new List<IOccupancyBatchRule>
            {
                new OccupancyBatchRequiredRegionRule("occupancy batch needs a value"),
                new OccupancyBatchAllowedCodeRule(new List<string> { "tiered", "pending" }),
                new OccupancyBatchStartsAtRangeRule(255, 255),
                new OccupancyBatchRequiredCodeRule("occupancy batch needs this field"),
                new OccupancyBatchCodePrefixRule("Q"),
                new OccupancyBatchDepartmentLengthRule(86400),
            });
    }
}
