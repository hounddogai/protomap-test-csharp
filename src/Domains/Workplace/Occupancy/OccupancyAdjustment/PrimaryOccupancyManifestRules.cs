namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>A rule a primary occupancy manifest must pass. Check returns its problems, or none.</summary>
public interface IPrimaryOccupancyManifestRule
{
    List<string> Check(PrimaryOccupancyManifest item);
}

public sealed record PrimaryOccupancyManifestRequiredNameRule(string Message) : IPrimaryOccupancyManifestRule
{
    public List<string> Check(PrimaryOccupancyManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOccupancyManifestNameLengthRule(int MaxLength) : IPrimaryOccupancyManifestRule
{
    public List<string> Check(PrimaryOccupancyManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOccupancyManifestRequiredNotesRule(string Message) : IPrimaryOccupancyManifestRule
{
    public List<string> Check(PrimaryOccupancyManifest item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary occupancy manifest.</summary>
public sealed record PrimaryOccupancyManifestValidator(List<IPrimaryOccupancyManifestRule> Rules)
{
    public List<string> Validate(PrimaryOccupancyManifest item)
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

    public void RequireValid(PrimaryOccupancyManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryOccupancyManifest> items)
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

/// <summary>Functions of primary occupancy manifest rules.</summary>
public static class PrimaryOccupancyManifestRules
{
    public static PrimaryOccupancyManifestValidator DefaultPrimaryOccupancyManifestValidator()
    {
        return new PrimaryOccupancyManifestValidator(
            new List<IPrimaryOccupancyManifestRule>
            {
                new PrimaryOccupancyManifestRequiredNameRule("primary occupancy manifest needs a value"),
                new PrimaryOccupancyManifestNameLengthRule(3600),
                new PrimaryOccupancyManifestRequiredNotesRule("primary occupancy manifest needs this field"),
            });
    }
}
