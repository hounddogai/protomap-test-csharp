namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>A rule a default laptop adjustment must pass. Check returns its problems, or none.</summary>
public interface IDefaultLaptopAdjustmentRule
{
    List<string> Check(DefaultLaptopAdjustment item);
}

public sealed record DefaultLaptopAdjustmentRequiredLabelRule(string Message) : IDefaultLaptopAdjustmentRule
{
    public List<string> Check(DefaultLaptopAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopAdjustmentPinnedGuardRule(int Limit) : IDefaultLaptopAdjustmentRule
{
    public List<string> Check(DefaultLaptopAdjustment item)
    {
        if (item.Pinned && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopAdjustmentRequiredCodeRule(string Message) : IDefaultLaptopAdjustmentRule
{
    public List<string> Check(DefaultLaptopAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default laptop adjustment.</summary>
public sealed record DefaultLaptopAdjustmentValidator(List<IDefaultLaptopAdjustmentRule> Rules)
{
    public List<string> Validate(DefaultLaptopAdjustment item)
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

    public void RequireValid(DefaultLaptopAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLaptopAdjustment> items)
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

/// <summary>Functions of default laptop adjustment rules.</summary>
public static class DefaultLaptopAdjustmentRules
{
    public static DefaultLaptopAdjustmentValidator DefaultDefaultLaptopAdjustmentValidator()
    {
        return new DefaultLaptopAdjustmentValidator(
            new List<IDefaultLaptopAdjustmentRule>
            {
                new DefaultLaptopAdjustmentRequiredLabelRule("default laptop adjustment needs this field"),
                new DefaultLaptopAdjustmentPinnedGuardRule(255),
                new DefaultLaptopAdjustmentRequiredCodeRule("default laptop adjustment needs an entry"),
            });
    }
}
