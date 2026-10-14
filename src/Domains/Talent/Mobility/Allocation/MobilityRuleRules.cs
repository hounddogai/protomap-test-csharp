namespace Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>A rule a mobility rule must pass. Check returns its problems, or none.</summary>
public interface IMobilityRuleRule
{
    List<string> Check(MobilityRule item);
}

public sealed record MobilityRuleCodeLengthRule(int MaxLength) : IMobilityRuleRule
{
    public List<string> Check(MobilityRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityRuleCodePrefixRule(string Prefix) : IMobilityRuleRule
{
    public List<string> Check(MobilityRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityRuleAmountRangeRule(double Min, double Max) : IMobilityRuleRule
{
    public List<string> Check(MobilityRule item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a mobility rule.</summary>
public sealed record MobilityRuleValidator(List<IMobilityRuleRule> Rules)
{
    public List<string> Validate(MobilityRule item)
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

    public void RequireValid(MobilityRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityRule> items)
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

/// <summary>Functions of mobility rule rules.</summary>
public static class MobilityRuleRules
{
    public static MobilityRuleValidator DefaultMobilityRuleValidator()
    {
        return new MobilityRuleValidator(
            new List<IMobilityRuleRule>
            {
                new MobilityRuleCodeLengthRule(16),
                new MobilityRuleCodePrefixRule("ACM"),
                new MobilityRuleAmountRangeRule(8.7, 551.9),
            });
    }
}
