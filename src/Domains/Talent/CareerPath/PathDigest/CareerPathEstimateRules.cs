namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path estimate must pass. Check returns its problems, or none.</summary>
public interface ICareerPathEstimateRule
{
    List<string> Check(CareerPathEstimate item);
}

public sealed record CareerPathEstimateRequiredNameRule(string Message) : ICareerPathEstimateRule
{
    public List<string> Check(CareerPathEstimate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathEstimateAllowedUnitRule(List<string> Allowed) : ICareerPathEstimateRule
{
    public List<string> Check(CareerPathEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is stale" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathEstimateAttemptsRangeRule(int Min, int Max) : ICareerPathEstimateRule
{
    public List<string> Check(CareerPathEstimate item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathEstimateRequiredUnitRule(string Message) : ICareerPathEstimateRule
{
    public List<string> Check(CareerPathEstimate item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathEstimateCodePrefixRule(string Prefix) : ICareerPathEstimateRule
{
    public List<string> Check(CareerPathEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path estimate.</summary>
public sealed record CareerPathEstimateValidator(List<ICareerPathEstimateRule> Rules)
{
    public List<string> Validate(CareerPathEstimate item)
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

    public void RequireValid(CareerPathEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathEstimate> items)
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

/// <summary>Functions of career path estimate rules.</summary>
public static class CareerPathEstimateRules
{
    public static CareerPathEstimateValidator DefaultCareerPathEstimateValidator()
    {
        return new CareerPathEstimateValidator(
            new List<ICareerPathEstimateRule>
            {
                new CareerPathEstimateRequiredNameRule("career path estimate needs an entry"),
                new CareerPathEstimateAllowedUnitRule(new List<string> { "scheduled", "legacy" }),
                new CareerPathEstimateAttemptsRangeRule(64, 255),
                new CareerPathEstimateRequiredUnitRule("career path estimate needs an entry"),
                new CareerPathEstimateCodePrefixRule("Q"),
            });
    }
}
