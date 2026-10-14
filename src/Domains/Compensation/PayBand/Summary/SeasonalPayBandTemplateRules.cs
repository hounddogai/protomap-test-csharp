namespace Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>A rule a seasonal pay band template must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPayBandTemplateRule
{
    List<string> Check(SeasonalPayBandTemplate item);
}

public sealed record SeasonalPayBandTemplateCreatedAtRangeRule(int Min, int Max) : ISeasonalPayBandTemplateRule
{
    public List<string> Check(SeasonalPayBandTemplate item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandTemplateCodePrefixRule(string Prefix) : ISeasonalPayBandTemplateRule
{
    public List<string> Check(SeasonalPayBandTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandTemplateRequiredNameRule(string Message) : ISeasonalPayBandTemplateRule
{
    public List<string> Check(SeasonalPayBandTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandTemplateCodeLengthRule(int MaxLength) : ISeasonalPayBandTemplateRule
{
    public List<string> Check(SeasonalPayBandTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandTemplateNameLengthRule(int MaxLength) : ISeasonalPayBandTemplateRule
{
    public List<string> Check(SeasonalPayBandTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandTemplateStartsAtRangeRule(int Min, int Max) : ISeasonalPayBandTemplateRule
{
    public List<string> Check(SeasonalPayBandTemplate item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal pay band template.</summary>
public sealed record SeasonalPayBandTemplateValidator(List<ISeasonalPayBandTemplateRule> Rules)
{
    public List<string> Validate(SeasonalPayBandTemplate item)
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

    public void RequireValid(SeasonalPayBandTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPayBandTemplate> items)
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

/// <summary>Functions of seasonal pay band template rules.</summary>
public static class SeasonalPayBandTemplateRules
{
    public static SeasonalPayBandTemplateValidator DefaultSeasonalPayBandTemplateValidator()
    {
        return new SeasonalPayBandTemplateValidator(
            new List<ISeasonalPayBandTemplateRule>
            {
                new SeasonalPayBandTemplateCreatedAtRangeRule(16, 86400),
                new SeasonalPayBandTemplateCodePrefixRule("REF"),
                new SeasonalPayBandTemplateRequiredNameRule("seasonal pay band template needs a value"),
                new SeasonalPayBandTemplateCodeLengthRule(64),
                new SeasonalPayBandTemplateNameLengthRule(120),
                new SeasonalPayBandTemplateStartsAtRangeRule(120, 120),
            });
    }
}
