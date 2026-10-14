namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback policy ack request must pass. Check returns its problems, or none.</summary>
public interface IFallbackPolicyAckRequestRule
{
    List<string> Check(FallbackPolicyAckRequest item);
}

public sealed record FallbackPolicyAckRequestRequiredCodeRule(string Message) : IFallbackPolicyAckRequestRule
{
    public List<string> Check(FallbackPolicyAckRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckRequestMarginRangeRule(double Min, double Max) : IFallbackPolicyAckRequestRule
{
    public List<string> Check(FallbackPolicyAckRequest item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckRequestRequiredLabelRule(string Message) : IFallbackPolicyAckRequestRule
{
    public List<string> Check(FallbackPolicyAckRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckRequestRatingRangeRule(double Min, double Max) : IFallbackPolicyAckRequestRule
{
    public List<string> Check(FallbackPolicyAckRequest item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPolicyAckRequestAllowedLabelRule(List<string> Allowed) : IFallbackPolicyAckRequestRule
{
    public List<string> Check(FallbackPolicyAckRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback policy ack request.</summary>
public sealed record FallbackPolicyAckRequestValidator(List<IFallbackPolicyAckRequestRule> Rules)
{
    public List<string> Validate(FallbackPolicyAckRequest item)
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

    public void RequireValid(FallbackPolicyAckRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPolicyAckRequest> items)
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

/// <summary>Functions of fallback policy ack request rules.</summary>
public static class FallbackPolicyAckRequestRules
{
    public static FallbackPolicyAckRequestValidator DefaultFallbackPolicyAckRequestValidator()
    {
        return new FallbackPolicyAckRequestValidator(
            new List<IFallbackPolicyAckRequestRule>
            {
                new FallbackPolicyAckRequestRequiredCodeRule("fallback policy ack request needs an entry"),
                new FallbackPolicyAckRequestMarginRangeRule(7.2, 1392.1),
                new FallbackPolicyAckRequestRequiredLabelRule("fallback policy ack request needs a value"),
                new FallbackPolicyAckRequestRatingRangeRule(1.4, 2072.2),
                new FallbackPolicyAckRequestAllowedLabelRule(new List<string> { "adaptive" }),
            });
    }
}
