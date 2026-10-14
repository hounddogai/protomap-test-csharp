namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop estimate must pass. Check returns its problems, or none.</summary>
public interface ILaptopEstimateRule
{
    List<string> Check(LaptopEstimate item);
}

public sealed record LaptopEstimateExpiresAtRangeRule(int Min, int Max) : ILaptopEstimateRule
{
    public List<string> Check(LaptopEstimate item)
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

public sealed record LaptopEstimateAllowedNameRule(List<string> Allowed) : ILaptopEstimateRule
{
    public List<string> Check(LaptopEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

public sealed record LaptopEstimateNameLengthRule(int MaxLength) : ILaptopEstimateRule
{
    public List<string> Check(LaptopEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopEstimateCategoryLengthRule(int MaxLength) : ILaptopEstimateRule
{
    public List<string> Check(LaptopEstimate item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop estimate.</summary>
public sealed record LaptopEstimateValidator(List<ILaptopEstimateRule> Rules)
{
    public List<string> Validate(LaptopEstimate item)
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

    public void RequireValid(LaptopEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopEstimate> items)
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

/// <summary>Functions of laptop estimate rules.</summary>
public static class LaptopEstimateRules
{
    public static LaptopEstimateValidator DefaultLaptopEstimateValidator()
    {
        return new LaptopEstimateValidator(
            new List<ILaptopEstimateRule>
            {
                new LaptopEstimateExpiresAtRangeRule(32, 80),
                new LaptopEstimateAllowedNameRule(new List<string> { "strict" }),
                new LaptopEstimateNameLengthRule(120),
                new LaptopEstimateCategoryLengthRule(86400),
            });
    }
}
