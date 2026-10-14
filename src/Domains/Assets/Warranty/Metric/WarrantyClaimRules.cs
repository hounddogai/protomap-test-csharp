namespace Acme.Workforce.Assets.Warranty.Metric;

/// <summary>A rule a warranty claim must pass. Check returns its problems, or none.</summary>
public interface IWarrantyClaimRule
{
    List<string> Check(WarrantyClaim item);
}

public sealed record WarrantyClaimRatingRangeRule(double Min, double Max) : IWarrantyClaimRule
{
    public List<string> Check(WarrantyClaim item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyClaimFteRangeRule(double Min, double Max) : IWarrantyClaimRule
{
    public List<string> Check(WarrantyClaim item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WarrantyClaimExternalRefLengthRule(int MaxLength) : IWarrantyClaimRule
{
    public List<string> Check(WarrantyClaim item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty claim.</summary>
public sealed record WarrantyClaimValidator(List<IWarrantyClaimRule> Rules)
{
    public List<string> Validate(WarrantyClaim item)
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

    public void RequireValid(WarrantyClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyClaim> items)
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

/// <summary>Functions of warranty claim rules.</summary>
public static class WarrantyClaimRules
{
    public static WarrantyClaimValidator DefaultWarrantyClaimValidator()
    {
        return new WarrantyClaimValidator(
            new List<IWarrantyClaimRule>
            {
                new WarrantyClaimRatingRangeRule(9.0, 1666.8),
                new WarrantyClaimFteRangeRule(5.7, 2310.3),
                new WarrantyClaimExternalRefLengthRule(1000),
            });
    }
}
