namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary policy ack mapping must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPolicyAckMappingRule
{
    List<string> Check(PrimaryPolicyAckMapping item);
}

public sealed record PrimaryPolicyAckMappingRequiredCodeRule(string Message) : IPrimaryPolicyAckMappingRule
{
    public List<string> Check(PrimaryPolicyAckMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckMappingAllowedLocationRule(List<string> Allowed) : IPrimaryPolicyAckMappingRule
{
    public List<string> Check(PrimaryPolicyAckMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckMappingRequiredLabelRule(string Message) : IPrimaryPolicyAckMappingRule
{
    public List<string> Check(PrimaryPolicyAckMapping item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckMappingRequiredSkillRule(string Message) : IPrimaryPolicyAckMappingRule
{
    public List<string> Check(PrimaryPolicyAckMapping item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckMappingCodePrefixRule(string Prefix) : IPrimaryPolicyAckMappingRule
{
    public List<string> Check(PrimaryPolicyAckMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckMappingRatingRangeRule(double Min, double Max) : IPrimaryPolicyAckMappingRule
{
    public List<string> Check(PrimaryPolicyAckMapping item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary policy ack mapping.</summary>
public sealed record PrimaryPolicyAckMappingValidator(List<IPrimaryPolicyAckMappingRule> Rules)
{
    public List<string> Validate(PrimaryPolicyAckMapping item)
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

    public void RequireValid(PrimaryPolicyAckMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPolicyAckMapping> items)
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

/// <summary>Functions of primary policy ack mapping rules.</summary>
public static class PrimaryPolicyAckMappingRules
{
    public static PrimaryPolicyAckMappingValidator DefaultPrimaryPolicyAckMappingValidator()
    {
        return new PrimaryPolicyAckMappingValidator(
            new List<IPrimaryPolicyAckMappingRule>
            {
                new PrimaryPolicyAckMappingRequiredCodeRule("primary policy ack mapping needs an entry"),
                new PrimaryPolicyAckMappingAllowedLocationRule(new List<string> { "tiered", "regional", "manual" }),
                new PrimaryPolicyAckMappingRequiredLabelRule("primary policy ack mapping needs this field"),
                new PrimaryPolicyAckMappingRequiredSkillRule("primary policy ack mapping needs this field"),
                new PrimaryPolicyAckMappingCodePrefixRule("REF"),
                new PrimaryPolicyAckMappingRatingRangeRule(3.1, 411.9),
            });
    }
}
