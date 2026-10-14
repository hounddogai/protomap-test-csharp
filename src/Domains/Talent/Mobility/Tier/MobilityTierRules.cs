namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility tier must pass. Check returns its problems, or none.</summary>
public interface IMobilityTierRule
{
    List<string> Check(MobilityTier item);
}

public sealed record MobilityTierRequiredLabelRule(string Message) : IMobilityTierRule
{
    public List<string> Check(MobilityTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityTierLabelLengthRule(int MaxLength) : IMobilityTierRule
{
    public List<string> Check(MobilityTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityTierCostRangeRule(double Min, double Max) : IMobilityTierRule
{
    public List<string> Check(MobilityTier item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityTierRequiredSalaryBandRule(string Message) : IMobilityTierRule
{
    public List<string> Check(MobilityTier item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityTierAllowedLabelRule(List<string> Allowed) : IMobilityTierRule
{
    public List<string> Check(MobilityTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record MobilityTierUnitLengthRule(int MaxLength) : IMobilityTierRule
{
    public List<string> Check(MobilityTier item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility tier.</summary>
public sealed record MobilityTierValidator(List<IMobilityTierRule> Rules)
{
    public List<string> Validate(MobilityTier item)
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

    public void RequireValid(MobilityTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityTier> items)
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

/// <summary>Functions of mobility tier rules.</summary>
public static class MobilityTierRules
{
    public static MobilityTierValidator DefaultMobilityTierValidator()
    {
        return new MobilityTierValidator(
            new List<IMobilityTierRule>
            {
                new MobilityTierRequiredLabelRule("mobility tier needs an entry"),
                new MobilityTierLabelLengthRule(120),
                new MobilityTierCostRangeRule(1.2, 4881.8),
                new MobilityTierRequiredSalaryBandRule("mobility tier needs an entry"),
                new MobilityTierAllowedLabelRule(new List<string> { "scheduled" }),
                new MobilityTierUnitLengthRule(64),
            });
    }
}
