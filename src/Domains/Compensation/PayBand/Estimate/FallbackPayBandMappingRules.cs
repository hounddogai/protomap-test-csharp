namespace Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>A rule a fallback pay band mapping must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayBandMappingRule
{
    List<string> Check(FallbackPayBandMapping item);
}

public sealed record FallbackPayBandMappingSalaryBandLengthRule(int MaxLength) : IFallbackPayBandMappingRule
{
    public List<string> Check(FallbackPayBandMapping item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandMappingCodeLengthRule(int MaxLength) : IFallbackPayBandMappingRule
{
    public List<string> Check(FallbackPayBandMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandMappingDurationMinutesRangeRule(int Min, int Max) : IFallbackPayBandMappingRule
{
    public List<string> Check(FallbackPayBandMapping item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandMappingRequiredSalaryBandRule(string Message) : IFallbackPayBandMappingRule
{
    public List<string> Check(FallbackPayBandMapping item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback pay band mapping.</summary>
public sealed record FallbackPayBandMappingValidator(List<IFallbackPayBandMappingRule> Rules)
{
    public List<string> Validate(FallbackPayBandMapping item)
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

    public void RequireValid(FallbackPayBandMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayBandMapping> items)
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

/// <summary>Functions of fallback pay band mapping rules.</summary>
public static class FallbackPayBandMappingRules
{
    public static FallbackPayBandMappingValidator DefaultFallbackPayBandMappingValidator()
    {
        return new FallbackPayBandMappingValidator(
            new List<IFallbackPayBandMappingRule>
            {
                new FallbackPayBandMappingSalaryBandLengthRule(86400),
                new FallbackPayBandMappingCodeLengthRule(120),
                new FallbackPayBandMappingDurationMinutesRangeRule(16, 80),
                new FallbackPayBandMappingRequiredSalaryBandRule("fallback pay band mapping needs an entry"),
            });
    }
}
