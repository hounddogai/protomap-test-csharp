namespace Acme.Workforce.Compliance.Certification.Adjustment;

/// <summary>A rule a weighted certification milestone must pass. Check returns its problems, or none.</summary>
public interface IWeightedCertificationMilestoneRule
{
    List<string> Check(WeightedCertificationMilestone item);
}

public sealed record WeightedCertificationMilestoneUpdatedAtRangeRule(int Min, int Max) : IWeightedCertificationMilestoneRule
{
    public List<string> Check(WeightedCertificationMilestone item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedCertificationMilestoneCodePrefixRule(string Prefix) : IWeightedCertificationMilestoneRule
{
    public List<string> Check(WeightedCertificationMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedCertificationMilestoneQuantityRangeRule(int Min, int Max) : IWeightedCertificationMilestoneRule
{
    public List<string> Check(WeightedCertificationMilestone item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted certification milestone.</summary>
public sealed record WeightedCertificationMilestoneValidator(List<IWeightedCertificationMilestoneRule> Rules)
{
    public List<string> Validate(WeightedCertificationMilestone item)
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

    public void RequireValid(WeightedCertificationMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedCertificationMilestone> items)
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

/// <summary>Functions of weighted certification milestone rules.</summary>
public static class WeightedCertificationMilestoneRules
{
    public static WeightedCertificationMilestoneValidator DefaultWeightedCertificationMilestoneValidator()
    {
        return new WeightedCertificationMilestoneValidator(
            new List<IWeightedCertificationMilestoneRule>
            {
                new WeightedCertificationMilestoneUpdatedAtRangeRule(120, 3600),
                new WeightedCertificationMilestoneCodePrefixRule("REF"),
                new WeightedCertificationMilestoneQuantityRangeRule(16, 3600),
            });
    }
}
