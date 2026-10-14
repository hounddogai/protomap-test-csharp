namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy bundle must pass. Check returns its problems, or none.</summary>
public interface IOccupancyBundleRule
{
    List<string> Check(OccupancyBundle item);
}

public sealed record OccupancyBundleFteRangeRule(double Min, double Max) : IOccupancyBundleRule
{
    public List<string> Check(OccupancyBundle item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBundleUpdatedAtRangeRule(int Min, int Max) : IOccupancyBundleRule
{
    public List<string> Check(OccupancyBundle item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBundleAllowedCostCenterRule(List<string> Allowed) : IOccupancyBundleRule
{
    public List<string> Check(OccupancyBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBundleCodeLengthRule(int MaxLength) : IOccupancyBundleRule
{
    public List<string> Check(OccupancyBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBundleRequiredTitleRule(string Message) : IOccupancyBundleRule
{
    public List<string> Check(OccupancyBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy bundle.</summary>
public sealed record OccupancyBundleValidator(List<IOccupancyBundleRule> Rules)
{
    public List<string> Validate(OccupancyBundle item)
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

    public void RequireValid(OccupancyBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyBundle> items)
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

/// <summary>Functions of occupancy bundle rules.</summary>
public static class OccupancyBundleRules
{
    public static OccupancyBundleValidator DefaultOccupancyBundleValidator()
    {
        return new OccupancyBundleValidator(
            new List<IOccupancyBundleRule>
            {
                new OccupancyBundleFteRangeRule(4.8, 3389.6),
                new OccupancyBundleUpdatedAtRangeRule(32, 255),
                new OccupancyBundleAllowedCostCenterRule(new List<string> { "seasonal" }),
                new OccupancyBundleCodeLengthRule(3600),
                new OccupancyBundleRequiredTitleRule("occupancy bundle needs this field"),
            });
    }
}
