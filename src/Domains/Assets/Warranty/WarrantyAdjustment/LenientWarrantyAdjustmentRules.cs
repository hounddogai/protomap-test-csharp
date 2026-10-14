namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient warranty adjustment must pass. Check returns its problems, or none.</summary>
public interface ILenientWarrantyAdjustmentRule
{
    List<string> Check(LenientWarrantyAdjustment item);
}

public sealed record LenientWarrantyAdjustmentAmountRangeRule(double Min, double Max) : ILenientWarrantyAdjustmentRule
{
    public List<string> Check(LenientWarrantyAdjustment item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientWarrantyAdjustmentAllowedLabelRule(List<string> Allowed) : ILenientWarrantyAdjustmentRule
{
    public List<string> Check(LenientWarrantyAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

public sealed record LenientWarrantyAdjustmentRequiredCodeRule(string Message) : ILenientWarrantyAdjustmentRule
{
    public List<string> Check(LenientWarrantyAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient warranty adjustment.</summary>
public sealed record LenientWarrantyAdjustmentValidator(List<ILenientWarrantyAdjustmentRule> Rules)
{
    public List<string> Validate(LenientWarrantyAdjustment item)
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

    public void RequireValid(LenientWarrantyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientWarrantyAdjustment> items)
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

/// <summary>Functions of lenient warranty adjustment rules.</summary>
public static class LenientWarrantyAdjustmentRules
{
    public static LenientWarrantyAdjustmentValidator DefaultLenientWarrantyAdjustmentValidator()
    {
        return new LenientWarrantyAdjustmentValidator(
            new List<ILenientWarrantyAdjustmentRule>
            {
                new LenientWarrantyAdjustmentAmountRangeRule(6.6, 883.2),
                new LenientWarrantyAdjustmentAllowedLabelRule(new List<string> { "pending", "regional" }),
                new LenientWarrantyAdjustmentRequiredCodeRule("lenient warranty adjustment needs this field"),
            });
    }
}
