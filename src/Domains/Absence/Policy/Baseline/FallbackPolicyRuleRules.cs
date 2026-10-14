namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback policy rule must pass. Check returns its problems, or none.</summary>
public interface IFallbackPolicyRuleRule
{
    List<string> Check(FallbackPolicyRule item);
}

public sealed record FallbackPolicyRuleRequiredGradeRule(string Message) : IFallbackPolicyRuleRule
{
    public List<string> Check(FallbackPolicyRule item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyRuleAllowedContactEmailRule(List<string> Allowed) : IFallbackPolicyRuleRule
{
    public List<string> Check(FallbackPolicyRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyRuleRequiredContactEmailRule(string Message) : IFallbackPolicyRuleRule
{
    public List<string> Check(FallbackPolicyRule item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyRuleTitleLengthRule(int MaxLength) : IFallbackPolicyRuleRule
{
    public List<string> Check(FallbackPolicyRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyRuleMaximumRangeRule(double Min, double Max) : IFallbackPolicyRuleRule
{
    public List<string> Check(FallbackPolicyRule item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPolicyRuleFloorValueRangeRule(double Min, double Max) : IFallbackPolicyRuleRule
{
    public List<string> Check(FallbackPolicyRule item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback policy rule.</summary>
public sealed record FallbackPolicyRuleValidator(List<IFallbackPolicyRuleRule> Rules)
{
    public List<string> Validate(FallbackPolicyRule item)
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

    public void RequireValid(FallbackPolicyRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPolicyRule> items)
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

/// <summary>Functions of fallback policy rule rules.</summary>
public static class FallbackPolicyRuleRules
{
    public static FallbackPolicyRuleValidator DefaultFallbackPolicyRuleValidator()
    {
        return new FallbackPolicyRuleValidator(
            new List<IFallbackPolicyRuleRule>
            {
                new FallbackPolicyRuleRequiredGradeRule("fallback policy rule needs a value"),
                new FallbackPolicyRuleAllowedContactEmailRule(
                    new List<string> { "fallback", "weighted", "adaptive" }),
                new FallbackPolicyRuleRequiredContactEmailRule("fallback policy rule needs a value"),
                new FallbackPolicyRuleTitleLengthRule(3600),
                new FallbackPolicyRuleMaximumRangeRule(5.2, 3633.6),
                new FallbackPolicyRuleFloorValueRangeRule(3.6, 1574.5),
            });
    }
}
