namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty score must pass. Check returns its problems, or none.</summary>
public interface IWarrantyScoreRule
{
    List<string> Check(WarrantyScore item);
}

public sealed record WarrantyScoreMinimumRangeRule(double Min, double Max) : IWarrantyScoreRule
{
    public List<string> Check(WarrantyScore item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyScoreRequiredCodeRule(string Message) : IWarrantyScoreRule
{
    public List<string> Check(WarrantyScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyScoreAllowedDepartmentRule(List<string> Allowed) : IWarrantyScoreRule
{
    public List<string> Check(WarrantyScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyScoreQuantityRangeRule(int Min, int Max) : IWarrantyScoreRule
{
    public List<string> Check(WarrantyScore item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a warranty score.</summary>
public sealed record WarrantyScoreValidator(List<IWarrantyScoreRule> Rules)
{
    public List<string> Validate(WarrantyScore item)
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

    public void RequireValid(WarrantyScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyScore> items)
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

/// <summary>Functions of warranty score rules.</summary>
public static class WarrantyScoreRules
{
    public static WarrantyScoreValidator DefaultWarrantyScoreValidator()
    {
        return new WarrantyScoreValidator(
            new List<IWarrantyScoreRule>
            {
                new WarrantyScoreMinimumRangeRule(1.7, 1520.3),
                new WarrantyScoreRequiredCodeRule("warranty score needs a value"),
                new WarrantyScoreAllowedDepartmentRule(new List<string>()),
                new WarrantyScoreQuantityRangeRule(32, 64),
            });
    }
}
