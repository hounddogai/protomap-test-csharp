namespace Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>A rule a rate baseline must pass. Check returns its problems, or none.</summary>
public interface IRateBaselineRule
{
    List<string> Check(RateBaseline item);
}

public sealed record RateBaselineSalaryBandLengthRule(int MaxLength) : IRateBaselineRule
{
    public List<string> Check(RateBaseline item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateBaselineRequiredNameRule(string Message) : IRateBaselineRule
{
    public List<string> Check(RateBaseline item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateBaselineVisibleGuardRule(int Limit) : IRateBaselineRule
{
    public List<string> Check(RateBaseline item)
    {
        if (item.Visible && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RateBaselineRequiredLocaleRule(string Message) : IRateBaselineRule
{
    public List<string> Check(RateBaseline item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateBaselineRequiredCodeRule(string Message) : IRateBaselineRule
{
    public List<string> Check(RateBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate baseline.</summary>
public sealed record RateBaselineValidator(List<IRateBaselineRule> Rules)
{
    public List<string> Validate(RateBaseline item)
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

    public void RequireValid(RateBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateBaseline> items)
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

/// <summary>Functions of rate baseline rules.</summary>
public static class RateBaselineRules
{
    public static RateBaselineValidator DefaultRateBaselineValidator()
    {
        return new RateBaselineValidator(
            new List<IRateBaselineRule>
            {
                new RateBaselineSalaryBandLengthRule(80),
                new RateBaselineRequiredNameRule("rate baseline needs this field"),
                new RateBaselineVisibleGuardRule(80),
                new RateBaselineRequiredLocaleRule("rate baseline needs an entry"),
                new RateBaselineRequiredCodeRule("rate baseline needs an entry"),
            });
    }
}
