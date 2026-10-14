namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>A rule a pending warranty allocation must pass. Check returns its problems, or none.</summary>
public interface IPendingWarrantyAllocationRule
{
    List<string> Check(PendingWarrantyAllocation item);
}

public sealed record PendingWarrantyAllocationRegionLengthRule(int MaxLength) : IPendingWarrantyAllocationRule
{
    public List<string> Check(PendingWarrantyAllocation item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyAllocationRequiredCodeRule(string Message) : IPendingWarrantyAllocationRule
{
    public List<string> Check(PendingWarrantyAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyAllocationRequiredTitleRule(string Message) : IPendingWarrantyAllocationRule
{
    public List<string> Check(PendingWarrantyAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending warranty allocation.</summary>
public sealed record PendingWarrantyAllocationValidator(List<IPendingWarrantyAllocationRule> Rules)
{
    public List<string> Validate(PendingWarrantyAllocation item)
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

    public void RequireValid(PendingWarrantyAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingWarrantyAllocation> items)
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

/// <summary>Functions of pending warranty allocation rules.</summary>
public static class PendingWarrantyAllocationRules
{
    public static PendingWarrantyAllocationValidator DefaultPendingWarrantyAllocationValidator()
    {
        return new PendingWarrantyAllocationValidator(
            new List<IPendingWarrantyAllocationRule>
            {
                new PendingWarrantyAllocationRegionLengthRule(32),
                new PendingWarrantyAllocationRequiredCodeRule("pending warranty allocation needs this field"),
                new PendingWarrantyAllocationRequiredTitleRule("pending warranty allocation needs a value"),
            });
    }
}
