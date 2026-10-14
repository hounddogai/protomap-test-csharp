namespace Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>A rule a tiered occupancy allocation must pass. Check returns its problems, or none.</summary>
public interface ITieredOccupancyAllocationRule
{
    List<string> Check(TieredOccupancyAllocation item);
}

public sealed record TieredOccupancyAllocationRequiredManagerIdRule(string Message) : ITieredOccupancyAllocationRule
{
    public List<string> Check(TieredOccupancyAllocation item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredOccupancyAllocationRequiredContactEmailRule(string Message) : ITieredOccupancyAllocationRule
{
    public List<string> Check(TieredOccupancyAllocation item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredOccupancyAllocationCodeLengthRule(int MaxLength) : ITieredOccupancyAllocationRule
{
    public List<string> Check(TieredOccupancyAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredOccupancyAllocationContactEmailLengthRule(int MaxLength) : ITieredOccupancyAllocationRule
{
    public List<string> Check(TieredOccupancyAllocation item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered occupancy allocation.</summary>
public sealed record TieredOccupancyAllocationValidator(List<ITieredOccupancyAllocationRule> Rules)
{
    public List<string> Validate(TieredOccupancyAllocation item)
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

    public void RequireValid(TieredOccupancyAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredOccupancyAllocation> items)
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

/// <summary>Functions of tiered occupancy allocation rules.</summary>
public static class TieredOccupancyAllocationRules
{
    public static TieredOccupancyAllocationValidator DefaultTieredOccupancyAllocationValidator()
    {
        return new TieredOccupancyAllocationValidator(
            new List<ITieredOccupancyAllocationRule>
            {
                new TieredOccupancyAllocationRequiredManagerIdRule("tiered occupancy allocation needs this field"),
                new TieredOccupancyAllocationRequiredContactEmailRule("tiered occupancy allocation needs a value"),
                new TieredOccupancyAllocationCodeLengthRule(120),
                new TieredOccupancyAllocationContactEmailLengthRule(1000),
            });
    }
}
