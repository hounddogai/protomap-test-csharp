namespace Acme.Workforce.Assets.Laptop.Rollup;

/// <summary>A rule a weighted laptop rollup must pass. Check returns its problems, or none.</summary>
public interface IWeightedLaptopRollupRule
{
    List<string> Check(WeightedLaptopRollup item);
}

public sealed record WeightedLaptopRollupRequiredNameRule(string Message) : IWeightedLaptopRollupRule
{
    public List<string> Check(WeightedLaptopRollup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopRollupRequiredCostCenterRule(string Message) : IWeightedLaptopRollupRule
{
    public List<string> Check(WeightedLaptopRollup item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopRollupDescriptionLengthRule(int MaxLength) : IWeightedLaptopRollupRule
{
    public List<string> Check(WeightedLaptopRollup item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted laptop rollup.</summary>
public sealed record WeightedLaptopRollupValidator(List<IWeightedLaptopRollupRule> Rules)
{
    public List<string> Validate(WeightedLaptopRollup item)
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

    public void RequireValid(WeightedLaptopRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedLaptopRollup> items)
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

/// <summary>Functions of weighted laptop rollup rules.</summary>
public static class WeightedLaptopRollupRules
{
    public static WeightedLaptopRollupValidator DefaultWeightedLaptopRollupValidator()
    {
        return new WeightedLaptopRollupValidator(
            new List<IWeightedLaptopRollupRule>
            {
                new WeightedLaptopRollupRequiredNameRule("weighted laptop rollup needs a value"),
                new WeightedLaptopRollupRequiredCostCenterRule("weighted laptop rollup needs this field"),
                new WeightedLaptopRollupDescriptionLengthRule(80),
            });
    }
}
