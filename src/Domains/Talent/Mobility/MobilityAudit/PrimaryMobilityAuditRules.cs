namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>A rule a primary mobility audit must pass. Check returns its problems, or none.</summary>
public interface IPrimaryMobilityAuditRule
{
    List<string> Check(PrimaryMobilityAudit item);
}

public sealed record PrimaryMobilityAuditCodePrefixRule(string Prefix) : IPrimaryMobilityAuditRule
{
    public List<string> Check(PrimaryMobilityAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMobilityAuditTitleLengthRule(int MaxLength) : IPrimaryMobilityAuditRule
{
    public List<string> Check(PrimaryMobilityAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMobilityAuditRequiredTeamRule(string Message) : IPrimaryMobilityAuditRule
{
    public List<string> Check(PrimaryMobilityAudit item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMobilityAuditTeamLengthRule(int MaxLength) : IPrimaryMobilityAuditRule
{
    public List<string> Check(PrimaryMobilityAudit item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMobilityAuditRequiredTitleRule(string Message) : IPrimaryMobilityAuditRule
{
    public List<string> Check(PrimaryMobilityAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMobilityAuditAmountRangeRule(double Min, double Max) : IPrimaryMobilityAuditRule
{
    public List<string> Check(PrimaryMobilityAudit item)
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

/// <summary>Runs every rule of a primary mobility audit.</summary>
public sealed record PrimaryMobilityAuditValidator(List<IPrimaryMobilityAuditRule> Rules)
{
    public List<string> Validate(PrimaryMobilityAudit item)
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

    public void RequireValid(PrimaryMobilityAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryMobilityAudit> items)
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

/// <summary>Functions of primary mobility audit rules.</summary>
public static class PrimaryMobilityAuditRules
{
    public static PrimaryMobilityAuditValidator DefaultPrimaryMobilityAuditValidator()
    {
        return new PrimaryMobilityAuditValidator(
            new List<IPrimaryMobilityAuditRule>
            {
                new PrimaryMobilityAuditCodePrefixRule("X"),
                new PrimaryMobilityAuditTitleLengthRule(86400),
                new PrimaryMobilityAuditRequiredTeamRule("primary mobility audit needs this field"),
                new PrimaryMobilityAuditTeamLengthRule(64),
                new PrimaryMobilityAuditRequiredTitleRule("primary mobility audit needs this field"),
                new PrimaryMobilityAuditAmountRangeRule(6.8, 1186.9),
            });
    }
}
