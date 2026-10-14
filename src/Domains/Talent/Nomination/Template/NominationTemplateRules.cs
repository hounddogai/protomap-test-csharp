namespace Acme.Workforce.Talent.Nomination.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination template must pass. Check returns its problems, or none.</summary>
public interface INominationTemplateRule
{
    List<string> Check(NominationTemplate item);
}

public sealed record NominationTemplateAttemptsRangeRule(int Min, int Max) : INominationTemplateRule
{
    public List<string> Check(NominationTemplate item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationTemplateAllowedCodeRule(List<string> Allowed) : INominationTemplateRule
{
    public List<string> Check(NominationTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record NominationTemplateRequiredCodeRule(string Message) : INominationTemplateRule
{
    public List<string> Check(NominationTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationTemplateCodePrefixRule(string Prefix) : INominationTemplateRule
{
    public List<string> Check(NominationTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record NominationTemplateRequiredNameRule(string Message) : INominationTemplateRule
{
    public List<string> Check(NominationTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationTemplateWeightRangeRule(double Min, double Max) : INominationTemplateRule
{
    public List<string> Check(NominationTemplate item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination template.</summary>
public sealed record NominationTemplateValidator(List<INominationTemplateRule> Rules)
{
    public List<string> Validate(NominationTemplate item)
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

    public void RequireValid(NominationTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationTemplate> items)
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

/// <summary>Functions of nomination template rules.</summary>
public static class NominationTemplateRules
{
    public static NominationTemplateValidator DefaultNominationTemplateValidator()
    {
        return new NominationTemplateValidator(
            new List<INominationTemplateRule>
            {
                new NominationTemplateAttemptsRangeRule(32, 3600),
                new NominationTemplateAllowedCodeRule(new List<string>()),
                new NominationTemplateRequiredCodeRule("nomination template needs an entry"),
                new NominationTemplateCodePrefixRule("REF"),
                new NominationTemplateRequiredNameRule("nomination template needs a value"),
                new NominationTemplateWeightRangeRule(3.7, 560.6),
            });
    }
}
