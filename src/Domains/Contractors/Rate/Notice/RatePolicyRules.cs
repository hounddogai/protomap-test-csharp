namespace Acme.Workforce.Contractors.Rate.Notice;

/// <summary>A rule a rate policy must pass. Check returns its problems, or none.</summary>
public interface IRatePolicyRule
{
    List<string> Check(RatePolicy item);
}

public sealed record RatePolicyRequiredLabelRule(string Message) : IRatePolicyRule
{
    public List<string> Check(RatePolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RatePolicyCodePrefixRule(string Prefix) : IRatePolicyRule
{
    public List<string> Check(RatePolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RatePolicyRequiredCodeRule(string Message) : IRatePolicyRule
{
    public List<string> Check(RatePolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RatePolicyTenureMonthsRangeRule(int Min, int Max) : IRatePolicyRule
{
    public List<string> Check(RatePolicy item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RatePolicyRequiredTeamRule(string Message) : IRatePolicyRule
{
    public List<string> Check(RatePolicy item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate policy.</summary>
public sealed record RatePolicyValidator(List<IRatePolicyRule> Rules)
{
    public List<string> Validate(RatePolicy item)
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

    public void RequireValid(RatePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RatePolicy> items)
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

/// <summary>Functions of rate policy rules.</summary>
public static class RatePolicyRules
{
    public static RatePolicyValidator DefaultRatePolicyValidator()
    {
        return new RatePolicyValidator(
            new List<IRatePolicyRule>
            {
                new RatePolicyRequiredLabelRule("rate policy needs an entry"),
                new RatePolicyCodePrefixRule("X"),
                new RatePolicyRequiredCodeRule("rate policy needs an entry"),
                new RatePolicyTenureMonthsRangeRule(120, 3600),
                new RatePolicyRequiredTeamRule("rate policy needs a value"),
            });
    }
}
