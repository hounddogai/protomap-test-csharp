namespace Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>A rule a lenient policy ack template must pass. Check returns its problems, or none.</summary>
public interface ILenientPolicyAckTemplateRule
{
    List<string> Check(LenientPolicyAckTemplate item);
}

public sealed record LenientPolicyAckTemplateDurationMinutesRangeRule(int Min, int Max) : ILenientPolicyAckTemplateRule
{
    public List<string> Check(LenientPolicyAckTemplate item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckTemplateCodePrefixRule(string Prefix) : ILenientPolicyAckTemplateRule
{
    public List<string> Check(LenientPolicyAckTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckTemplateRequiredExternalRefRule(string Message) : ILenientPolicyAckTemplateRule
{
    public List<string> Check(LenientPolicyAckTemplate item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient policy ack template.</summary>
public sealed record LenientPolicyAckTemplateValidator(List<ILenientPolicyAckTemplateRule> Rules)
{
    public List<string> Validate(LenientPolicyAckTemplate item)
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

    public void RequireValid(LenientPolicyAckTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPolicyAckTemplate> items)
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

/// <summary>Functions of lenient policy ack template rules.</summary>
public static class LenientPolicyAckTemplateRules
{
    public static LenientPolicyAckTemplateValidator DefaultLenientPolicyAckTemplateValidator()
    {
        return new LenientPolicyAckTemplateValidator(
            new List<ILenientPolicyAckTemplateRule>
            {
                new LenientPolicyAckTemplateDurationMinutesRangeRule(255, 255),
                new LenientPolicyAckTemplateCodePrefixRule("Q"),
                new LenientPolicyAckTemplateRequiredExternalRefRule("lenient policy ack template needs this field"),
            });
    }
}
