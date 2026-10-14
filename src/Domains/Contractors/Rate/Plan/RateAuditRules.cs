namespace Acme.Workforce.Contractors.Rate.Plan;

/// <summary>A rule a rate audit must pass. Check returns its problems, or none.</summary>
public interface IRateAuditRule
{
    List<string> Check(RateAudit item);
}

public sealed record RateAuditRequiredNameRule(string Message) : IRateAuditRule
{
    public List<string> Check(RateAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateAuditRateRangeRule(double Min, double Max) : IRateAuditRule
{
    public List<string> Check(RateAudit item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateAuditCodePrefixRule(string Prefix) : IRateAuditRule
{
    public List<string> Check(RateAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate audit.</summary>
public sealed record RateAuditValidator(List<IRateAuditRule> Rules)
{
    public List<string> Validate(RateAudit item)
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

    public void RequireValid(RateAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateAudit> items)
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

/// <summary>Functions of rate audit rules.</summary>
public static class RateAuditRules
{
    public static RateAuditValidator DefaultRateAuditValidator()
    {
        return new RateAuditValidator(
            new List<IRateAuditRule>
            {
                new RateAuditRequiredNameRule("rate audit needs this field"),
                new RateAuditRateRangeRule(7.1, 900.3),
                new RateAuditCodePrefixRule("Q"),
            });
    }
}
