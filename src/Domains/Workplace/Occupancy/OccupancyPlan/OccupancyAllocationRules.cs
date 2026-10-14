namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>A rule a occupancy allocation must pass. Check returns its problems, or none.</summary>
public interface IOccupancyAllocationRule
{
    List<string> Check(OccupancyAllocation item);
}

public sealed record OccupancyAllocationLocaleLengthRule(int MaxLength) : IOccupancyAllocationRule
{
    public List<string> Check(OccupancyAllocation item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAllocationCodeLengthRule(int MaxLength) : IOccupancyAllocationRule
{
    public List<string> Check(OccupancyAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAllocationLabelLengthRule(int MaxLength) : IOccupancyAllocationRule
{
    public List<string> Check(OccupancyAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAllocationRequiredLocaleRule(string Message) : IOccupancyAllocationRule
{
    public List<string> Check(OccupancyAllocation item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAllocationRequiredLabelRule(string Message) : IOccupancyAllocationRule
{
    public List<string> Check(OccupancyAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy allocation.</summary>
public sealed record OccupancyAllocationValidator(List<IOccupancyAllocationRule> Rules)
{
    public List<string> Validate(OccupancyAllocation item)
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

    public void RequireValid(OccupancyAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyAllocation> items)
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

/// <summary>Functions of occupancy allocation rules.</summary>
public static class OccupancyAllocationRules
{
    public static OccupancyAllocationValidator DefaultOccupancyAllocationValidator()
    {
        return new OccupancyAllocationValidator(
            new List<IOccupancyAllocationRule>
            {
                new OccupancyAllocationLocaleLengthRule(80),
                new OccupancyAllocationCodeLengthRule(120),
                new OccupancyAllocationLabelLengthRule(16),
                new OccupancyAllocationRequiredLocaleRule("occupancy allocation needs an entry"),
                new OccupancyAllocationRequiredLabelRule("occupancy allocation needs a value"),
            });
    }
}
