namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>A rule a adaptive occupancy agreement must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveOccupancyAgreementRule
{
    List<string> Check(AdaptiveOccupancyAgreement item);
}

public sealed record AdaptiveOccupancyAgreementNameLengthRule(int MaxLength) : IAdaptiveOccupancyAgreementRule
{
    public List<string> Check(AdaptiveOccupancyAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOccupancyAgreementCodeLengthRule(int MaxLength) : IAdaptiveOccupancyAgreementRule
{
    public List<string> Check(AdaptiveOccupancyAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOccupancyAgreementDiscountRangeRule(double Min, double Max) : IAdaptiveOccupancyAgreementRule
{
    public List<string> Check(AdaptiveOccupancyAgreement item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOccupancyAgreementRequiredCodeRule(string Message) : IAdaptiveOccupancyAgreementRule
{
    public List<string> Check(AdaptiveOccupancyAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOccupancyAgreementCodePrefixRule(string Prefix) : IAdaptiveOccupancyAgreementRule
{
    public List<string> Check(AdaptiveOccupancyAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive occupancy agreement.</summary>
public sealed record AdaptiveOccupancyAgreementValidator(List<IAdaptiveOccupancyAgreementRule> Rules)
{
    public List<string> Validate(AdaptiveOccupancyAgreement item)
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

    public void RequireValid(AdaptiveOccupancyAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveOccupancyAgreement> items)
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

/// <summary>Functions of adaptive occupancy agreement rules.</summary>
public static class AdaptiveOccupancyAgreementRules
{
    public static AdaptiveOccupancyAgreementValidator DefaultAdaptiveOccupancyAgreementValidator()
    {
        return new AdaptiveOccupancyAgreementValidator(
            new List<IAdaptiveOccupancyAgreementRule>
            {
                new AdaptiveOccupancyAgreementNameLengthRule(32),
                new AdaptiveOccupancyAgreementCodeLengthRule(1000),
                new AdaptiveOccupancyAgreementDiscountRangeRule(7.5, 3796.5),
                new AdaptiveOccupancyAgreementRequiredCodeRule("adaptive occupancy agreement needs this field"),
                new AdaptiveOccupancyAgreementCodePrefixRule("REF"),
            });
    }
}
