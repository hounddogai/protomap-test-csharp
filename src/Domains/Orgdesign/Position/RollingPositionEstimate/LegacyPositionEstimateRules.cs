namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy position estimate must pass. Check returns its problems, or none.</summary>
public interface ILegacyPositionEstimateRule
{
    List<string> Check(LegacyPositionEstimate item);
}

public sealed record LegacyPositionEstimateAllowedCurrencyRule(List<string> Allowed) : ILegacyPositionEstimateRule
{
    public List<string> Check(LegacyPositionEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionEstimateDiscountRangeRule(double Min, double Max) : ILegacyPositionEstimateRule
{
    public List<string> Check(LegacyPositionEstimate item)
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

public sealed record LegacyPositionEstimateLabelLengthRule(int MaxLength) : ILegacyPositionEstimateRule
{
    public List<string> Check(LegacyPositionEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionEstimateRequiredCurrencyRule(string Message) : ILegacyPositionEstimateRule
{
    public List<string> Check(LegacyPositionEstimate item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionEstimateRateRangeRule(double Min, double Max) : ILegacyPositionEstimateRule
{
    public List<string> Check(LegacyPositionEstimate item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy position estimate.</summary>
public sealed record LegacyPositionEstimateValidator(List<ILegacyPositionEstimateRule> Rules)
{
    public List<string> Validate(LegacyPositionEstimate item)
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

    public void RequireValid(LegacyPositionEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPositionEstimate> items)
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

/// <summary>Functions of legacy position estimate rules.</summary>
public static class LegacyPositionEstimateRules
{
    public static LegacyPositionEstimateValidator DefaultLegacyPositionEstimateValidator()
    {
        return new LegacyPositionEstimateValidator(
            new List<ILegacyPositionEstimateRule>
            {
                new LegacyPositionEstimateAllowedCurrencyRule(new List<string> { "rolling", "scheduled", "legacy" }),
                new LegacyPositionEstimateDiscountRangeRule(0.9, 951.7),
                new LegacyPositionEstimateLabelLengthRule(255),
                new LegacyPositionEstimateRequiredCurrencyRule("legacy position estimate needs this field"),
                new LegacyPositionEstimateRateRangeRule(8.5, 3300.6),
            });
    }
}
