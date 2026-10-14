namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

/// <summary>A rule a fallback mobility mapping must pass. Check returns its problems, or none.</summary>
public interface IFallbackMobilityMappingRule
{
    List<string> Check(FallbackMobilityMapping item);
}

public sealed record FallbackMobilityMappingCodePrefixRule(string Prefix) : IFallbackMobilityMappingRule
{
    public List<string> Check(FallbackMobilityMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMobilityMappingAmountRangeRule(double Min, double Max) : IFallbackMobilityMappingRule
{
    public List<string> Check(FallbackMobilityMapping item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMobilityMappingRequiredContactEmailRule(string Message) : IFallbackMobilityMappingRule
{
    public List<string> Check(FallbackMobilityMapping item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackMobilityMappingTenureMonthsRangeRule(int Min, int Max) : IFallbackMobilityMappingRule
{
    public List<string> Check(FallbackMobilityMapping item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackMobilityMappingLabelLengthRule(int MaxLength) : IFallbackMobilityMappingRule
{
    public List<string> Check(FallbackMobilityMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback mobility mapping.</summary>
public sealed record FallbackMobilityMappingValidator(List<IFallbackMobilityMappingRule> Rules)
{
    public List<string> Validate(FallbackMobilityMapping item)
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

    public void RequireValid(FallbackMobilityMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackMobilityMapping> items)
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

/// <summary>Functions of fallback mobility mapping rules.</summary>
public static class FallbackMobilityMappingRules
{
    public static FallbackMobilityMappingValidator DefaultFallbackMobilityMappingValidator()
    {
        return new FallbackMobilityMappingValidator(
            new List<IFallbackMobilityMappingRule>
            {
                new FallbackMobilityMappingCodePrefixRule("OPS"),
                new FallbackMobilityMappingAmountRangeRule(3.4, 1401.2),
                new FallbackMobilityMappingRequiredContactEmailRule("fallback mobility mapping needs an entry"),
                new FallbackMobilityMappingTenureMonthsRangeRule(80, 3600),
                new FallbackMobilityMappingLabelLengthRule(3600),
            });
    }
}
