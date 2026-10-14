namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>A rule a fallback license seat audit must pass. Check returns its problems, or none.</summary>
public interface IFallbackLicenseSeatAuditRule
{
    List<string> Check(FallbackLicenseSeatAudit item);
}

public sealed record FallbackLicenseSeatAuditRequiredContactEmailRule(string Message) : IFallbackLicenseSeatAuditRule
{
    public List<string> Check(FallbackLicenseSeatAudit item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatAuditLimitRangeRule(int Min, int Max) : IFallbackLicenseSeatAuditRule
{
    public List<string> Check(FallbackLicenseSeatAudit item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackLicenseSeatAuditCountryLengthRule(int MaxLength) : IFallbackLicenseSeatAuditRule
{
    public List<string> Check(FallbackLicenseSeatAudit item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatAuditTagsLimitRule(int MaxCount, List<string> Banned) : IFallbackLicenseSeatAuditRule
{
    public List<string> Check(FallbackLicenseSeatAudit item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback license seat audit.</summary>
public sealed record FallbackLicenseSeatAuditValidator(List<IFallbackLicenseSeatAuditRule> Rules)
{
    public List<string> Validate(FallbackLicenseSeatAudit item)
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

    public void RequireValid(FallbackLicenseSeatAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackLicenseSeatAudit> items)
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

/// <summary>Functions of fallback license seat audit rules.</summary>
public static class FallbackLicenseSeatAuditRules
{
    public static FallbackLicenseSeatAuditValidator DefaultFallbackLicenseSeatAuditValidator()
    {
        return new FallbackLicenseSeatAuditValidator(
            new List<IFallbackLicenseSeatAuditRule>
            {
                new FallbackLicenseSeatAuditRequiredContactEmailRule("fallback license seat audit needs a value"),
                new FallbackLicenseSeatAuditLimitRangeRule(1000, 86400),
                new FallbackLicenseSeatAuditCountryLengthRule(86400),
                new FallbackLicenseSeatAuditTagsLimitRule(86400, new List<string> { "weighted", "capped" }),
            });
    }
}
