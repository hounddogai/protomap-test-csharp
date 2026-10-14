namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient asset return adjustment must pass. Check returns its problems, or none.</summary>
public interface ILenientAssetReturnAdjustmentRule
{
    List<string> Check(LenientAssetReturnAdjustment item);
}

public sealed record LenientAssetReturnAdjustmentCreatedAtRangeRule(int Min, int Max) : ILenientAssetReturnAdjustmentRule
{
    public List<string> Check(LenientAssetReturnAdjustment item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientAssetReturnAdjustmentAllowedCodeRule(List<string> Allowed) : ILenientAssetReturnAdjustmentRule
{
    public List<string> Check(LenientAssetReturnAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record LenientAssetReturnAdjustmentCodeLengthRule(int MaxLength) : ILenientAssetReturnAdjustmentRule
{
    public List<string> Check(LenientAssetReturnAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAssetReturnAdjustmentLimitRangeRule(int Min, int Max) : ILenientAssetReturnAdjustmentRule
{
    public List<string> Check(LenientAssetReturnAdjustment item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientAssetReturnAdjustmentDiscountRangeRule(double Min, double Max) : ILenientAssetReturnAdjustmentRule
{
    public List<string> Check(LenientAssetReturnAdjustment item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient asset return adjustment.</summary>
public sealed record LenientAssetReturnAdjustmentValidator(List<ILenientAssetReturnAdjustmentRule> Rules)
{
    public List<string> Validate(LenientAssetReturnAdjustment item)
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

    public void RequireValid(LenientAssetReturnAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientAssetReturnAdjustment> items)
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

/// <summary>Functions of lenient asset return adjustment rules.</summary>
public static class LenientAssetReturnAdjustmentRules
{
    public static LenientAssetReturnAdjustmentValidator DefaultLenientAssetReturnAdjustmentValidator()
    {
        return new LenientAssetReturnAdjustmentValidator(
            new List<ILenientAssetReturnAdjustmentRule>
            {
                new LenientAssetReturnAdjustmentCreatedAtRangeRule(16, 16),
                new LenientAssetReturnAdjustmentAllowedCodeRule(new List<string> { "regional", "tiered" }),
                new LenientAssetReturnAdjustmentCodeLengthRule(86400),
                new LenientAssetReturnAdjustmentLimitRangeRule(1000, 3600),
                new LenientAssetReturnAdjustmentDiscountRangeRule(7.6, 2757.7),
            });
    }
}
