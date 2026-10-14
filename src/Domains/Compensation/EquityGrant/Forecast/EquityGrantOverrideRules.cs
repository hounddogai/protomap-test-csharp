namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

/// <summary>A rule a equity grant override must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantOverrideRule
{
    List<string> Check(EquityGrantOverride item);
}

public sealed record EquityGrantOverrideRatioRangeRule(double Min, double Max) : IEquityGrantOverrideRule
{
    public List<string> Check(EquityGrantOverride item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantOverrideNameLengthRule(int MaxLength) : IEquityGrantOverrideRule
{
    public List<string> Check(EquityGrantOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantOverrideSalaryBandLengthRule(int MaxLength) : IEquityGrantOverrideRule
{
    public List<string> Check(EquityGrantOverride item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantOverrideDiscountRangeRule(double Min, double Max) : IEquityGrantOverrideRule
{
    public List<string> Check(EquityGrantOverride item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantOverrideCostCenterLengthRule(int MaxLength) : IEquityGrantOverrideRule
{
    public List<string> Check(EquityGrantOverride item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantOverrideExpiresAtRangeRule(int Min, int Max) : IEquityGrantOverrideRule
{
    public List<string> Check(EquityGrantOverride item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant override.</summary>
public sealed record EquityGrantOverrideValidator(List<IEquityGrantOverrideRule> Rules)
{
    public List<string> Validate(EquityGrantOverride item)
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

    public void RequireValid(EquityGrantOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantOverride> items)
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

/// <summary>Functions of equity grant override rules.</summary>
public static class EquityGrantOverrideRules
{
    public static EquityGrantOverrideValidator DefaultEquityGrantOverrideValidator()
    {
        return new EquityGrantOverrideValidator(
            new List<IEquityGrantOverrideRule>
            {
                new EquityGrantOverrideRatioRangeRule(9.5, 1898.5),
                new EquityGrantOverrideNameLengthRule(1000),
                new EquityGrantOverrideSalaryBandLengthRule(1000),
                new EquityGrantOverrideDiscountRangeRule(1.8, 3381.3),
                new EquityGrantOverrideCostCenterLengthRule(255),
                new EquityGrantOverrideExpiresAtRangeRule(16, 80),
            });
    }
}
