namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered rate request must pass. Check returns its problems, or none.</summary>
public interface ITieredRateRequestRule
{
    List<string> Check(TieredRateRequest item);
}

public sealed record TieredRateRequestRequiredEmployeeIdRule(string Message) : ITieredRateRequestRule
{
    public List<string> Check(TieredRateRequest item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredRateRequestRequiredTeamRule(string Message) : ITieredRateRequestRule
{
    public List<string> Check(TieredRateRequest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredRateRequestAllowedTeamRule(List<string> Allowed) : ITieredRateRequestRule
{
    public List<string> Check(TieredRateRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record TieredRateRequestTeamLengthRule(int MaxLength) : ITieredRateRequestRule
{
    public List<string> Check(TieredRateRequest item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredRateRequestEmployeeIdLengthRule(int MaxLength) : ITieredRateRequestRule
{
    public List<string> Check(TieredRateRequest item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered rate request.</summary>
public sealed record TieredRateRequestValidator(List<ITieredRateRequestRule> Rules)
{
    public List<string> Validate(TieredRateRequest item)
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

    public void RequireValid(TieredRateRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredRateRequest> items)
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

/// <summary>Functions of tiered rate request rules.</summary>
public static class TieredRateRequestRules
{
    public static TieredRateRequestValidator DefaultTieredRateRequestValidator()
    {
        return new TieredRateRequestValidator(
            new List<ITieredRateRequestRule>
            {
                new TieredRateRequestRequiredEmployeeIdRule("tiered rate request needs an entry"),
                new TieredRateRequestRequiredTeamRule("tiered rate request needs a value"),
                new TieredRateRequestAllowedTeamRule(new List<string> { "scheduled", "rolling", "rolling" }),
                new TieredRateRequestTeamLengthRule(32),
                new TieredRateRequestEmployeeIdLengthRule(3600),
            });
    }
}
