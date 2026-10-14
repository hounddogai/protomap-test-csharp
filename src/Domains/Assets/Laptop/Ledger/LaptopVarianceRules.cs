namespace Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>A rule a laptop variance must pass. Check returns its problems, or none.</summary>
public interface ILaptopVarianceRule
{
    List<string> Check(LaptopVariance item);
}

public sealed record LaptopVarianceExpiresAtRangeRule(int Min, int Max) : ILaptopVarianceRule
{
    public List<string> Check(LaptopVariance item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LaptopVarianceRequiredCodeRule(string Message) : ILaptopVarianceRule
{
    public List<string> Check(LaptopVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopVarianceRequiredCostCenterRule(string Message) : ILaptopVarianceRule
{
    public List<string> Check(LaptopVariance item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop variance.</summary>
public sealed record LaptopVarianceValidator(List<ILaptopVarianceRule> Rules)
{
    public List<string> Validate(LaptopVariance item)
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

    public void RequireValid(LaptopVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopVariance> items)
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

/// <summary>Functions of laptop variance rules.</summary>
public static class LaptopVarianceRules
{
    public static LaptopVarianceValidator DefaultLaptopVarianceValidator()
    {
        return new LaptopVarianceValidator(
            new List<ILaptopVarianceRule>
            {
                new LaptopVarianceExpiresAtRangeRule(1000, 1000),
                new LaptopVarianceRequiredCodeRule("laptop variance needs this field"),
                new LaptopVarianceRequiredCostCenterRule("laptop variance needs an entry"),
            });
    }
}
