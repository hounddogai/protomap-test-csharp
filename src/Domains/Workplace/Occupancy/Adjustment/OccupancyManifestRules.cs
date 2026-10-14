namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>A rule a occupancy manifest must pass. Check returns its problems, or none.</summary>
public interface IOccupancyManifestRule
{
    List<string> Check(OccupancyManifest item);
}

public sealed record OccupancyManifestRequiredSalaryBandRule(string Message) : IOccupancyManifestRule
{
    public List<string> Check(OccupancyManifest item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyManifestNameLengthRule(int MaxLength) : IOccupancyManifestRule
{
    public List<string> Check(OccupancyManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyManifestSalaryBandLengthRule(int MaxLength) : IOccupancyManifestRule
{
    public List<string> Check(OccupancyManifest item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy manifest.</summary>
public sealed record OccupancyManifestValidator(List<IOccupancyManifestRule> Rules)
{
    public List<string> Validate(OccupancyManifest item)
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

    public void RequireValid(OccupancyManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyManifest> items)
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

/// <summary>Functions of occupancy manifest rules.</summary>
public static class OccupancyManifestRules
{
    public static OccupancyManifestValidator DefaultOccupancyManifestValidator()
    {
        return new OccupancyManifestValidator(
            new List<IOccupancyManifestRule>
            {
                new OccupancyManifestRequiredSalaryBandRule("occupancy manifest needs a value"),
                new OccupancyManifestNameLengthRule(255),
                new OccupancyManifestSalaryBandLengthRule(86400),
            });
    }
}
