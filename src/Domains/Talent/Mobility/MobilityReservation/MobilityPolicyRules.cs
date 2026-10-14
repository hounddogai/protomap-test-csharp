namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

/// <summary>A rule a mobility policy must pass. Check returns its problems, or none.</summary>
public interface IMobilityPolicyRule
{
    List<string> Check(MobilityPolicy item);
}

public sealed record MobilityPolicyWeightRangeRule(double Min, double Max) : IMobilityPolicyRule
{
    public List<string> Check(MobilityPolicy item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityPolicyCodeLengthRule(int MaxLength) : IMobilityPolicyRule
{
    public List<string> Check(MobilityPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityPolicyBudgetRangeRule(double Min, double Max) : IMobilityPolicyRule
{
    public List<string> Check(MobilityPolicy item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MobilityPolicyCodePrefixRule(string Prefix) : IMobilityPolicyRule
{
    public List<string> Check(MobilityPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityPolicyNameLengthRule(int MaxLength) : IMobilityPolicyRule
{
    public List<string> Check(MobilityPolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityPolicyRequiredCodeRule(string Message) : IMobilityPolicyRule
{
    public List<string> Check(MobilityPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility policy.</summary>
public sealed record MobilityPolicyValidator(List<IMobilityPolicyRule> Rules)
{
    public List<string> Validate(MobilityPolicy item)
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

    public void RequireValid(MobilityPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityPolicy> items)
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

/// <summary>Functions of mobility policy rules.</summary>
public static class MobilityPolicyRules
{
    public static MobilityPolicyValidator DefaultMobilityPolicyValidator()
    {
        return new MobilityPolicyValidator(
            new List<IMobilityPolicyRule>
            {
                new MobilityPolicyWeightRangeRule(3.7, 2020.6),
                new MobilityPolicyCodeLengthRule(80),
                new MobilityPolicyBudgetRangeRule(2.3, 2327.8),
                new MobilityPolicyCodePrefixRule("OPS"),
                new MobilityPolicyNameLengthRule(64),
                new MobilityPolicyRequiredCodeRule("mobility policy needs this field"),
            });
    }
}
