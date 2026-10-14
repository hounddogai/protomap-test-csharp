namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop profile must pass. Check returns its problems, or none.</summary>
public interface ILaptopProfileRule
{
    List<string> Check(LaptopProfile item);
}

public sealed record LaptopProfileAllowedTitleRule(List<string> Allowed) : ILaptopProfileRule
{
    public List<string> Check(LaptopProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record LaptopProfileDiscountRangeRule(double Min, double Max) : ILaptopProfileRule
{
    public List<string> Check(LaptopProfile item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LaptopProfileCostCenterLengthRule(int MaxLength) : ILaptopProfileRule
{
    public List<string> Check(LaptopProfile item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopProfileFloorValueRangeRule(double Min, double Max) : ILaptopProfileRule
{
    public List<string> Check(LaptopProfile item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopProfileDurationMinutesRangeRule(int Min, int Max) : ILaptopProfileRule
{
    public List<string> Check(LaptopProfile item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop profile.</summary>
public sealed record LaptopProfileValidator(List<ILaptopProfileRule> Rules)
{
    public List<string> Validate(LaptopProfile item)
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

    public void RequireValid(LaptopProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopProfile> items)
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

/// <summary>Functions of laptop profile rules.</summary>
public static class LaptopProfileRules
{
    public static LaptopProfileValidator DefaultLaptopProfileValidator()
    {
        return new LaptopProfileValidator(
            new List<ILaptopProfileRule>
            {
                new LaptopProfileAllowedTitleRule(new List<string> { "primary" }),
                new LaptopProfileDiscountRangeRule(5.4, 449.7),
                new LaptopProfileCostCenterLengthRule(16),
                new LaptopProfileFloorValueRangeRule(7.2, 3246.1),
                new LaptopProfileDurationMinutesRangeRule(255, 255),
            });
    }
}
