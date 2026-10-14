namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict warranty tier must pass. Check returns its problems, or none.</summary>
public interface IStrictWarrantyTierRule
{
    List<string> Check(StrictWarrantyTier item);
}

public sealed record StrictWarrantyTierCreatedAtRangeRule(int Min, int Max) : IStrictWarrantyTierRule
{
    public List<string> Check(StrictWarrantyTier item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyTierDepartmentLengthRule(int MaxLength) : IStrictWarrantyTierRule
{
    public List<string> Check(StrictWarrantyTier item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyTierCodePrefixRule(string Prefix) : IStrictWarrantyTierRule
{
    public List<string> Check(StrictWarrantyTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyTierAllowedOwnerIdRule(List<string> Allowed) : IStrictWarrantyTierRule
{
    public List<string> Check(StrictWarrantyTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict warranty tier.</summary>
public sealed record StrictWarrantyTierValidator(List<IStrictWarrantyTierRule> Rules)
{
    public List<string> Validate(StrictWarrantyTier item)
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

    public void RequireValid(StrictWarrantyTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictWarrantyTier> items)
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

/// <summary>Functions of strict warranty tier rules.</summary>
public static class StrictWarrantyTierRules
{
    public static StrictWarrantyTierValidator DefaultStrictWarrantyTierValidator()
    {
        return new StrictWarrantyTierValidator(
            new List<IStrictWarrantyTierRule>
            {
                new StrictWarrantyTierCreatedAtRangeRule(64, 120),
                new StrictWarrantyTierDepartmentLengthRule(255),
                new StrictWarrantyTierCodePrefixRule("ACM"),
                new StrictWarrantyTierAllowedOwnerIdRule(new List<string> { "fallback" }),
            });
    }
}
