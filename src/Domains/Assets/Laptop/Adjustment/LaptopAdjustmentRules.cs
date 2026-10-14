namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop adjustment must pass. Check returns its problems, or none.</summary>
public interface ILaptopAdjustmentRule
{
    List<string> Check(LaptopAdjustment item);
}

public sealed record LaptopAdjustmentPinnedGuardRule(double Limit) : ILaptopAdjustmentRule
{
    public List<string> Check(LaptopAdjustment item)
    {
        if (item.Pinned && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record LaptopAdjustmentCodeLengthRule(int MaxLength) : ILaptopAdjustmentRule
{
    public List<string> Check(LaptopAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopAdjustmentRateRangeRule(double Min, double Max) : ILaptopAdjustmentRule
{
    public List<string> Check(LaptopAdjustment item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LaptopAdjustmentAllowedCodeRule(List<string> Allowed) : ILaptopAdjustmentRule
{
    public List<string> Check(LaptopAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop adjustment.</summary>
public sealed record LaptopAdjustmentValidator(List<ILaptopAdjustmentRule> Rules)
{
    public List<string> Validate(LaptopAdjustment item)
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

    public void RequireValid(LaptopAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopAdjustment> items)
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

/// <summary>Functions of laptop adjustment rules.</summary>
public static class LaptopAdjustmentRules
{
    public static LaptopAdjustmentValidator DefaultLaptopAdjustmentValidator()
    {
        return new LaptopAdjustmentValidator(
            new List<ILaptopAdjustmentRule>
            {
                new LaptopAdjustmentPinnedGuardRule(4.7),
                new LaptopAdjustmentCodeLengthRule(80),
                new LaptopAdjustmentRateRangeRule(2.7, 1494.4),
                new LaptopAdjustmentAllowedCodeRule(new List<string> { "tiered" }),
            });
    }
}
