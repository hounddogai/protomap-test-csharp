namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal merit cycle profile must pass. Check returns its problems, or none.</summary>
public interface ISeasonalMeritCycleProfileRule
{
    List<string> Check(SeasonalMeritCycleProfile item);
}

public sealed record SeasonalMeritCycleProfileAllowedTeamRule(List<string> Allowed) : ISeasonalMeritCycleProfileRule
{
    public List<string> Check(SeasonalMeritCycleProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMeritCycleProfileRequiredCurrencyRule(string Message) : ISeasonalMeritCycleProfileRule
{
    public List<string> Check(SeasonalMeritCycleProfile item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMeritCycleProfileChecksumLengthRule(int MaxLength) : ISeasonalMeritCycleProfileRule
{
    public List<string> Check(SeasonalMeritCycleProfile item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMeritCycleProfileRequiredChecksumRule(string Message) : ISeasonalMeritCycleProfileRule
{
    public List<string> Check(SeasonalMeritCycleProfile item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal merit cycle profile.</summary>
public sealed record SeasonalMeritCycleProfileValidator(List<ISeasonalMeritCycleProfileRule> Rules)
{
    public List<string> Validate(SeasonalMeritCycleProfile item)
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

    public void RequireValid(SeasonalMeritCycleProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalMeritCycleProfile> items)
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

/// <summary>Functions of seasonal merit cycle profile rules.</summary>
public static class SeasonalMeritCycleProfileRules
{
    public static SeasonalMeritCycleProfileValidator DefaultSeasonalMeritCycleProfileValidator()
    {
        return new SeasonalMeritCycleProfileValidator(
            new List<ISeasonalMeritCycleProfileRule>
            {
                new SeasonalMeritCycleProfileAllowedTeamRule(new List<string> { "lenient", "default", "pending" }),
                new SeasonalMeritCycleProfileRequiredCurrencyRule("seasonal merit cycle profile needs this field"),
                new SeasonalMeritCycleProfileChecksumLengthRule(16),
                new SeasonalMeritCycleProfileRequiredChecksumRule("seasonal merit cycle profile needs an entry"),
            });
    }
}
