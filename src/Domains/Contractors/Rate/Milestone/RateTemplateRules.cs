namespace Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>A rule a rate template must pass. Check returns its problems, or none.</summary>
public interface IRateTemplateRule
{
    List<string> Check(RateTemplate item);
}

public sealed record RateTemplateSourceLengthRule(int MaxLength) : IRateTemplateRule
{
    public List<string> Check(RateTemplate item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateTemplateNameLengthRule(int MaxLength) : IRateTemplateRule
{
    public List<string> Check(RateTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateTemplateLocaleLengthRule(int MaxLength) : IRateTemplateRule
{
    public List<string> Check(RateTemplate item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateTemplateRequiredLocaleRule(string Message) : IRateTemplateRule
{
    public List<string> Check(RateTemplate item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateTemplateAttemptsRangeRule(int Min, int Max) : IRateTemplateRule
{
    public List<string> Check(RateTemplate item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RateTemplateGraceDaysRangeRule(int Min, int Max) : IRateTemplateRule
{
    public List<string> Check(RateTemplate item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate template.</summary>
public sealed record RateTemplateValidator(List<IRateTemplateRule> Rules)
{
    public List<string> Validate(RateTemplate item)
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

    public void RequireValid(RateTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateTemplate> items)
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

/// <summary>Functions of rate template rules.</summary>
public static class RateTemplateRules
{
    public static RateTemplateValidator DefaultRateTemplateValidator()
    {
        return new RateTemplateValidator(
            new List<IRateTemplateRule>
            {
                new RateTemplateSourceLengthRule(16),
                new RateTemplateNameLengthRule(255),
                new RateTemplateLocaleLengthRule(120),
                new RateTemplateRequiredLocaleRule("rate template needs an entry"),
                new RateTemplateAttemptsRangeRule(16, 3600),
                new RateTemplateGraceDaysRangeRule(80, 120),
            });
    }
}
