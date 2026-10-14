namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>A rule a regional pay run template must pass. Check returns its problems, or none.</summary>
public interface IRegionalPayRunTemplateRule
{
    List<string> Check(RegionalPayRunTemplate item);
}

public sealed record RegionalPayRunTemplateCodePrefixRule(string Prefix) : IRegionalPayRunTemplateRule
{
    public List<string> Check(RegionalPayRunTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunTemplateChannelLengthRule(int MaxLength) : IRegionalPayRunTemplateRule
{
    public List<string> Check(RegionalPayRunTemplate item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunTemplatePhoneLengthRule(int MaxLength) : IRegionalPayRunTemplateRule
{
    public List<string> Check(RegionalPayRunTemplate item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunTemplateRequiredPhoneRule(string Message) : IRegionalPayRunTemplateRule
{
    public List<string> Check(RegionalPayRunTemplate item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional pay run template.</summary>
public sealed record RegionalPayRunTemplateValidator(List<IRegionalPayRunTemplateRule> Rules)
{
    public List<string> Validate(RegionalPayRunTemplate item)
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

    public void RequireValid(RegionalPayRunTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPayRunTemplate> items)
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

/// <summary>Functions of regional pay run template rules.</summary>
public static class RegionalPayRunTemplateRules
{
    public static RegionalPayRunTemplateValidator DefaultRegionalPayRunTemplateValidator()
    {
        return new RegionalPayRunTemplateValidator(
            new List<IRegionalPayRunTemplateRule>
            {
                new RegionalPayRunTemplateCodePrefixRule("ACM"),
                new RegionalPayRunTemplateChannelLengthRule(1000),
                new RegionalPayRunTemplatePhoneLengthRule(120),
                new RegionalPayRunTemplateRequiredPhoneRule("regional pay run template needs an entry"),
            });
    }
}
