namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive certification adjustment must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCertificationAdjustmentRule
{
    List<string> Check(AdaptiveCertificationAdjustment item);
}

public sealed record AdaptiveCertificationAdjustmentCeilingRangeRule(double Min, double Max) : IAdaptiveCertificationAdjustmentRule
{
    public List<string> Check(AdaptiveCertificationAdjustment item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCertificationAdjustmentAllowedUnitRule(List<string> Allowed) : IAdaptiveCertificationAdjustmentRule
{
    public List<string> Check(AdaptiveCertificationAdjustment item)
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

public sealed record AdaptiveCertificationAdjustmentNameLengthRule(int MaxLength) : IAdaptiveCertificationAdjustmentRule
{
    public List<string> Check(AdaptiveCertificationAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive certification adjustment.</summary>
public sealed record AdaptiveCertificationAdjustmentValidator(List<IAdaptiveCertificationAdjustmentRule> Rules)
{
    public List<string> Validate(AdaptiveCertificationAdjustment item)
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

    public void RequireValid(AdaptiveCertificationAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCertificationAdjustment> items)
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

/// <summary>Functions of adaptive certification adjustment rules.</summary>
public static class AdaptiveCertificationAdjustmentRules
{
    public static AdaptiveCertificationAdjustmentValidator DefaultAdaptiveCertificationAdjustmentValidator()
    {
        return new AdaptiveCertificationAdjustmentValidator(
            new List<IAdaptiveCertificationAdjustmentRule>
            {
                new AdaptiveCertificationAdjustmentCeilingRangeRule(7.6, 4802.7),
                new AdaptiveCertificationAdjustmentAllowedUnitRule(new List<string> { "batched" }),
                new AdaptiveCertificationAdjustmentNameLengthRule(3600),
            });
    }
}
