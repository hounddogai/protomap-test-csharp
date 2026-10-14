namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty adjustment must pass. Check returns its problems, or none.</summary>
public interface IWarrantyAdjustmentRule
{
    List<string> Check(WarrantyAdjustment item);
}

public sealed record WarrantyAdjustmentLocaleLengthRule(int MaxLength) : IWarrantyAdjustmentRule
{
    public List<string> Check(WarrantyAdjustment item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAdjustmentRatingRangeRule(double Min, double Max) : IWarrantyAdjustmentRule
{
    public List<string> Check(WarrantyAdjustment item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAdjustmentAmountRangeRule(double Min, double Max) : IWarrantyAdjustmentRule
{
    public List<string> Check(WarrantyAdjustment item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WarrantyAdjustmentManagerIdLengthRule(int MaxLength) : IWarrantyAdjustmentRule
{
    public List<string> Check(WarrantyAdjustment item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAdjustmentAllowedTitleRule(List<string> Allowed) : IWarrantyAdjustmentRule
{
    public List<string> Check(WarrantyAdjustment item)
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

/// <summary>Runs every rule of a warranty adjustment.</summary>
public sealed record WarrantyAdjustmentValidator(List<IWarrantyAdjustmentRule> Rules)
{
    public List<string> Validate(WarrantyAdjustment item)
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

    public void RequireValid(WarrantyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyAdjustment> items)
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

/// <summary>Functions of warranty adjustment rules.</summary>
public static class WarrantyAdjustmentRules
{
    public static WarrantyAdjustmentValidator DefaultWarrantyAdjustmentValidator()
    {
        return new WarrantyAdjustmentValidator(
            new List<IWarrantyAdjustmentRule>
            {
                new WarrantyAdjustmentLocaleLengthRule(32),
                new WarrantyAdjustmentRatingRangeRule(5.0, 1593.5),
                new WarrantyAdjustmentAmountRangeRule(4.9, 3126.9),
                new WarrantyAdjustmentManagerIdLengthRule(120),
                new WarrantyAdjustmentAllowedTitleRule(new List<string> { "legacy", "seasonal", "rolling" }),
            });
    }
}
