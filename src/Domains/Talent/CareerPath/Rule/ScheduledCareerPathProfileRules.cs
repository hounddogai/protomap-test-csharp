namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled career path profile must pass. Check returns its problems, or none.</summary>
public interface IScheduledCareerPathProfileRule
{
    List<string> Check(ScheduledCareerPathProfile item);
}

public sealed record ScheduledCareerPathProfileCurrencyLengthRule(int MaxLength) : IScheduledCareerPathProfileRule
{
    public List<string> Check(ScheduledCareerPathProfile item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCareerPathProfileAllowedCountryRule(List<string> Allowed) : IScheduledCareerPathProfileRule
{
    public List<string> Check(ScheduledCareerPathProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCareerPathProfileBatchSizeRangeRule(int Min, int Max) : IScheduledCareerPathProfileRule
{
    public List<string> Check(ScheduledCareerPathProfile item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCareerPathProfileCountryLengthRule(int MaxLength) : IScheduledCareerPathProfileRule
{
    public List<string> Check(ScheduledCareerPathProfile item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCareerPathProfileMinimumRangeRule(double Min, double Max) : IScheduledCareerPathProfileRule
{
    public List<string> Check(ScheduledCareerPathProfile item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCareerPathProfileCreatedAtRangeRule(int Min, int Max) : IScheduledCareerPathProfileRule
{
    public List<string> Check(ScheduledCareerPathProfile item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled career path profile.</summary>
public sealed record ScheduledCareerPathProfileValidator(List<IScheduledCareerPathProfileRule> Rules)
{
    public List<string> Validate(ScheduledCareerPathProfile item)
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

    public void RequireValid(ScheduledCareerPathProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledCareerPathProfile> items)
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

/// <summary>Functions of scheduled career path profile rules.</summary>
public static class ScheduledCareerPathProfileRules
{
    public static ScheduledCareerPathProfileValidator DefaultScheduledCareerPathProfileValidator()
    {
        return new ScheduledCareerPathProfileValidator(
            new List<IScheduledCareerPathProfileRule>
            {
                new ScheduledCareerPathProfileCurrencyLengthRule(3600),
                new ScheduledCareerPathProfileAllowedCountryRule(new List<string> { "fallback", "weighted" }),
                new ScheduledCareerPathProfileBatchSizeRangeRule(3600, 3600),
                new ScheduledCareerPathProfileCountryLengthRule(64),
                new ScheduledCareerPathProfileMinimumRangeRule(7.9, 833.2),
                new ScheduledCareerPathProfileCreatedAtRangeRule(16, 80),
            });
    }
}
