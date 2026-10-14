namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive rate template must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveRateTemplateRule
{
    List<string> Check(AdaptiveRateTemplate item);
}

public sealed record AdaptiveRateTemplateRequiredLabelRule(string Message) : IAdaptiveRateTemplateRule
{
    public List<string> Check(AdaptiveRateTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRateTemplateAllowedChannelRule(List<string> Allowed) : IAdaptiveRateTemplateRule
{
    public List<string> Check(AdaptiveRateTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRateTemplateLabelLengthRule(int MaxLength) : IAdaptiveRateTemplateRule
{
    public List<string> Check(AdaptiveRateTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRateTemplateRequiredCodeRule(string Message) : IAdaptiveRateTemplateRule
{
    public List<string> Check(AdaptiveRateTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRateTemplateCodeLengthRule(int MaxLength) : IAdaptiveRateTemplateRule
{
    public List<string> Check(AdaptiveRateTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRateTemplateRequiredPhoneRule(string Message) : IAdaptiveRateTemplateRule
{
    public List<string> Check(AdaptiveRateTemplate item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive rate template.</summary>
public sealed record AdaptiveRateTemplateValidator(List<IAdaptiveRateTemplateRule> Rules)
{
    public List<string> Validate(AdaptiveRateTemplate item)
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

    public void RequireValid(AdaptiveRateTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveRateTemplate> items)
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

/// <summary>Functions of adaptive rate template rules.</summary>
public static class AdaptiveRateTemplateRules
{
    public static AdaptiveRateTemplateValidator DefaultAdaptiveRateTemplateValidator()
    {
        return new AdaptiveRateTemplateValidator(
            new List<IAdaptiveRateTemplateRule>
            {
                new AdaptiveRateTemplateRequiredLabelRule("adaptive rate template needs a value"),
                new AdaptiveRateTemplateAllowedChannelRule(new List<string> { "manual", "legacy" }),
                new AdaptiveRateTemplateLabelLengthRule(64),
                new AdaptiveRateTemplateRequiredCodeRule("adaptive rate template needs an entry"),
                new AdaptiveRateTemplateCodeLengthRule(16),
                new AdaptiveRateTemplateRequiredPhoneRule("adaptive rate template needs a value"),
            });
    }
}
