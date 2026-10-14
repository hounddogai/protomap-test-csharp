namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling cost model preference must pass. Check returns its problems, or none.</summary>
public interface IRollingCostModelPreferenceRule
{
    List<string> Check(RollingCostModelPreference item);
}

public sealed record RollingCostModelPreferenceExpiresAtRangeRule(int Min, int Max) : IRollingCostModelPreferenceRule
{
    public List<string> Check(RollingCostModelPreference item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelPreferenceAllowedTitleRule(List<string> Allowed) : IRollingCostModelPreferenceRule
{
    public List<string> Check(RollingCostModelPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelPreferenceTeamLengthRule(int MaxLength) : IRollingCostModelPreferenceRule
{
    public List<string> Check(RollingCostModelPreference item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelPreferenceTenureMonthsRangeRule(int Min, int Max) : IRollingCostModelPreferenceRule
{
    public List<string> Check(RollingCostModelPreference item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelPreferenceRequiredTitleRule(string Message) : IRollingCostModelPreferenceRule
{
    public List<string> Check(RollingCostModelPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling cost model preference.</summary>
public sealed record RollingCostModelPreferenceValidator(List<IRollingCostModelPreferenceRule> Rules)
{
    public List<string> Validate(RollingCostModelPreference item)
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

    public void RequireValid(RollingCostModelPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingCostModelPreference> items)
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

/// <summary>Functions of rolling cost model preference rules.</summary>
public static class RollingCostModelPreferenceRules
{
    public static RollingCostModelPreferenceValidator DefaultRollingCostModelPreferenceValidator()
    {
        return new RollingCostModelPreferenceValidator(
            new List<IRollingCostModelPreferenceRule>
            {
                new RollingCostModelPreferenceExpiresAtRangeRule(120, 255),
                new RollingCostModelPreferenceAllowedTitleRule(new List<string>()),
                new RollingCostModelPreferenceTeamLengthRule(86400),
                new RollingCostModelPreferenceTenureMonthsRangeRule(1000, 86400),
                new RollingCostModelPreferenceRequiredTitleRule("rolling cost model preference needs an entry"),
            });
    }
}
