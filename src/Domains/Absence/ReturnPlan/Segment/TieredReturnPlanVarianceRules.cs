namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered return plan variance must pass. Check returns its problems, or none.</summary>
public interface ITieredReturnPlanVarianceRule
{
    List<string> Check(TieredReturnPlanVariance item);
}

public sealed record TieredReturnPlanVarianceAllowedCodeRule(List<string> Allowed) : ITieredReturnPlanVarianceRule
{
    public List<string> Check(TieredReturnPlanVariance item)
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

public sealed record TieredReturnPlanVarianceCodeLengthRule(int MaxLength) : ITieredReturnPlanVarianceRule
{
    public List<string> Check(TieredReturnPlanVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanVarianceNameLengthRule(int MaxLength) : ITieredReturnPlanVarianceRule
{
    public List<string> Check(TieredReturnPlanVariance item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanVarianceCostRangeRule(double Min, double Max) : ITieredReturnPlanVarianceRule
{
    public List<string> Check(TieredReturnPlanVariance item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered return plan variance.</summary>
public sealed record TieredReturnPlanVarianceValidator(List<ITieredReturnPlanVarianceRule> Rules)
{
    public List<string> Validate(TieredReturnPlanVariance item)
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

    public void RequireValid(TieredReturnPlanVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredReturnPlanVariance> items)
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

/// <summary>Functions of tiered return plan variance rules.</summary>
public static class TieredReturnPlanVarianceRules
{
    public static TieredReturnPlanVarianceValidator DefaultTieredReturnPlanVarianceValidator()
    {
        return new TieredReturnPlanVarianceValidator(
            new List<ITieredReturnPlanVarianceRule>
            {
                new TieredReturnPlanVarianceAllowedCodeRule(new List<string>()),
                new TieredReturnPlanVarianceCodeLengthRule(3600),
                new TieredReturnPlanVarianceNameLengthRule(1000),
                new TieredReturnPlanVarianceCostRangeRule(0.2, 2192.7),
            });
    }
}
