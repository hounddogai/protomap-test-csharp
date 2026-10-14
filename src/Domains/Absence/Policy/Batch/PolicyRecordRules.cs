namespace Acme.Workforce.Absence.Policy.Batch;

/// <summary>A rule a policy record must pass. Check returns its problems, or none.</summary>
public interface IPolicyRecordRule
{
    List<string> Check(PolicyRecord item);
}

public sealed record PolicyRecordPinnedGuardRule(double Limit) : IPolicyRecordRule
{
    public List<string> Check(PolicyRecord item)
    {
        if (item.Pinned && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PolicyRecordCodeLengthRule(int MaxLength) : IPolicyRecordRule
{
    public List<string> Check(PolicyRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyRecordSkillLengthRule(int MaxLength) : IPolicyRecordRule
{
    public List<string> Check(PolicyRecord item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyRecordWeightRangeRule(double Min, double Max) : IPolicyRecordRule
{
    public List<string> Check(PolicyRecord item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyRecordRequiredSkillRule(string Message) : IPolicyRecordRule
{
    public List<string> Check(PolicyRecord item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyRecordCodePrefixRule(string Prefix) : IPolicyRecordRule
{
    public List<string> Check(PolicyRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy record.</summary>
public sealed record PolicyRecordValidator(List<IPolicyRecordRule> Rules)
{
    public List<string> Validate(PolicyRecord item)
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

    public void RequireValid(PolicyRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyRecord> items)
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

/// <summary>Functions of policy record rules.</summary>
public static class PolicyRecordRules
{
    public static PolicyRecordValidator DefaultPolicyRecordValidator()
    {
        return new PolicyRecordValidator(
            new List<IPolicyRecordRule>
            {
                new PolicyRecordPinnedGuardRule(0.4),
                new PolicyRecordCodeLengthRule(64),
                new PolicyRecordSkillLengthRule(255),
                new PolicyRecordWeightRangeRule(1.8, 4405.7),
                new PolicyRecordRequiredSkillRule("policy record needs this field"),
                new PolicyRecordCodePrefixRule("REF"),
            });
    }
}
