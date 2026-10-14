namespace Acme.Workforce.Talent.Mobility.Preference;

/// <summary>A rule a default mobility claim must pass. Check returns its problems, or none.</summary>
public interface IDefaultMobilityClaimRule
{
    List<string> Check(DefaultMobilityClaim item);
}

public sealed record DefaultMobilityClaimPinnedGuardRule(double Limit) : IDefaultMobilityClaimRule
{
    public List<string> Check(DefaultMobilityClaim item)
    {
        if (item.Pinned && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record DefaultMobilityClaimMaximumRangeRule(double Min, double Max) : IDefaultMobilityClaimRule
{
    public List<string> Check(DefaultMobilityClaim item)
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

public sealed record DefaultMobilityClaimQuantityRangeRule(int Min, int Max) : IDefaultMobilityClaimRule
{
    public List<string> Check(DefaultMobilityClaim item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default mobility claim.</summary>
public sealed record DefaultMobilityClaimValidator(List<IDefaultMobilityClaimRule> Rules)
{
    public List<string> Validate(DefaultMobilityClaim item)
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

    public void RequireValid(DefaultMobilityClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultMobilityClaim> items)
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

/// <summary>Functions of default mobility claim rules.</summary>
public static class DefaultMobilityClaimRules
{
    public static DefaultMobilityClaimValidator DefaultDefaultMobilityClaimValidator()
    {
        return new DefaultMobilityClaimValidator(
            new List<IDefaultMobilityClaimRule>
            {
                new DefaultMobilityClaimPinnedGuardRule(1.1),
                new DefaultMobilityClaimMaximumRangeRule(1.5, 1599.1),
                new DefaultMobilityClaimQuantityRangeRule(16, 3600),
            });
    }
}
