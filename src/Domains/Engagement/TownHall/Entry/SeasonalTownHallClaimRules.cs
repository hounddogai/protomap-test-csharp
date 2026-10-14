namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal town hall claim must pass. Check returns its problems, or none.</summary>
public interface ISeasonalTownHallClaimRule
{
    List<string> Check(SeasonalTownHallClaim item);
}

public sealed record SeasonalTownHallClaimAllowedCodeRule(List<string> Allowed) : ISeasonalTownHallClaimRule
{
    public List<string> Check(SeasonalTownHallClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTownHallClaimDiscountRangeRule(double Min, double Max) : ISeasonalTownHallClaimRule
{
    public List<string> Check(SeasonalTownHallClaim item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTownHallClaimAttemptsRangeRule(int Min, int Max) : ISeasonalTownHallClaimRule
{
    public List<string> Check(SeasonalTownHallClaim item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTownHallClaimRequiredLabelRule(string Message) : ISeasonalTownHallClaimRule
{
    public List<string> Check(SeasonalTownHallClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal town hall claim.</summary>
public sealed record SeasonalTownHallClaimValidator(List<ISeasonalTownHallClaimRule> Rules)
{
    public List<string> Validate(SeasonalTownHallClaim item)
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

    public void RequireValid(SeasonalTownHallClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalTownHallClaim> items)
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

/// <summary>Functions of seasonal town hall claim rules.</summary>
public static class SeasonalTownHallClaimRules
{
    public static SeasonalTownHallClaimValidator DefaultSeasonalTownHallClaimValidator()
    {
        return new SeasonalTownHallClaimValidator(
            new List<ISeasonalTownHallClaimRule>
            {
                new SeasonalTownHallClaimAllowedCodeRule(new List<string>()),
                new SeasonalTownHallClaimDiscountRangeRule(8.4, 3289.3),
                new SeasonalTownHallClaimAttemptsRangeRule(255, 1000),
                new SeasonalTownHallClaimRequiredLabelRule("seasonal town hall claim needs this field"),
            });
    }
}
