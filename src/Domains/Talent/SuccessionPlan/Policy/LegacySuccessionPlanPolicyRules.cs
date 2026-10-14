namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>A rule a legacy succession plan policy must pass. Check returns its problems, or none.</summary>
public interface ILegacySuccessionPlanPolicyRule
{
    List<string> Check(LegacySuccessionPlanPolicy item);
}

public sealed record LegacySuccessionPlanPolicyCodePrefixRule(string Prefix) : ILegacySuccessionPlanPolicyRule
{
    public List<string> Check(LegacySuccessionPlanPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySuccessionPlanPolicyRequiredCountryRule(string Message) : ILegacySuccessionPlanPolicyRule
{
    public List<string> Check(LegacySuccessionPlanPolicy item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacySuccessionPlanPolicySequenceRangeRule(int Min, int Max) : ILegacySuccessionPlanPolicyRule
{
    public List<string> Check(LegacySuccessionPlanPolicy item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacySuccessionPlanPolicySegmentLengthRule(int MaxLength) : ILegacySuccessionPlanPolicyRule
{
    public List<string> Check(LegacySuccessionPlanPolicy item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySuccessionPlanPolicyRequiredCodeRule(string Message) : ILegacySuccessionPlanPolicyRule
{
    public List<string> Check(LegacySuccessionPlanPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy succession plan policy.</summary>
public sealed record LegacySuccessionPlanPolicyValidator(List<ILegacySuccessionPlanPolicyRule> Rules)
{
    public List<string> Validate(LegacySuccessionPlanPolicy item)
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

    public void RequireValid(LegacySuccessionPlanPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacySuccessionPlanPolicy> items)
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

/// <summary>Functions of legacy succession plan policy rules.</summary>
public static class LegacySuccessionPlanPolicyRules
{
    public static LegacySuccessionPlanPolicyValidator DefaultLegacySuccessionPlanPolicyValidator()
    {
        return new LegacySuccessionPlanPolicyValidator(
            new List<ILegacySuccessionPlanPolicyRule>
            {
                new LegacySuccessionPlanPolicyCodePrefixRule("REF"),
                new LegacySuccessionPlanPolicyRequiredCountryRule("legacy succession plan policy needs an entry"),
                new LegacySuccessionPlanPolicySequenceRangeRule(255, 3600),
                new LegacySuccessionPlanPolicySegmentLengthRule(1000),
                new LegacySuccessionPlanPolicyRequiredCodeRule("legacy succession plan policy needs a value"),
            });
    }
}
