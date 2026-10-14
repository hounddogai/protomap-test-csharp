namespace Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>A rule a rate variance must pass. Check returns its problems, or none.</summary>
public interface IRateVarianceRule
{
    List<string> Check(RateVariance item);
}

public sealed record RateVarianceLocaleLengthRule(int MaxLength) : IRateVarianceRule
{
    public List<string> Check(RateVariance item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateVarianceRequiredLocaleRule(string Message) : IRateVarianceRule
{
    public List<string> Check(RateVariance item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateVarianceCodeLengthRule(int MaxLength) : IRateVarianceRule
{
    public List<string> Check(RateVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate variance.</summary>
public sealed record RateVarianceValidator(List<IRateVarianceRule> Rules)
{
    public List<string> Validate(RateVariance item)
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

    public void RequireValid(RateVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateVariance> items)
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

/// <summary>Functions of rate variance rules.</summary>
public static class RateVarianceRules
{
    public static RateVarianceValidator DefaultRateVarianceValidator()
    {
        return new RateVarianceValidator(
            new List<IRateVarianceRule>
            {
                new RateVarianceLocaleLengthRule(255),
                new RateVarianceRequiredLocaleRule("rate variance needs a value"),
                new RateVarianceCodeLengthRule(32),
            });
    }
}
