namespace Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>A rule a tiered nomination audit must pass. Check returns its problems, or none.</summary>
public interface ITieredNominationAuditRule
{
    List<string> Check(TieredNominationAudit item);
}

public sealed record TieredNominationAuditRequiredCodeRule(string Message) : ITieredNominationAuditRule
{
    public List<string> Check(TieredNominationAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationAuditFloorValueRangeRule(double Min, double Max) : ITieredNominationAuditRule
{
    public List<string> Check(TieredNominationAudit item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationAuditRevisionRangeRule(int Min, int Max) : ITieredNominationAuditRule
{
    public List<string> Check(TieredNominationAudit item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationAuditTitleLengthRule(int MaxLength) : ITieredNominationAuditRule
{
    public List<string> Check(TieredNominationAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationAuditMinimumRangeRule(double Min, double Max) : ITieredNominationAuditRule
{
    public List<string> Check(TieredNominationAudit item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationAuditRequiredPhoneRule(string Message) : ITieredNominationAuditRule
{
    public List<string> Check(TieredNominationAudit item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered nomination audit.</summary>
public sealed record TieredNominationAuditValidator(List<ITieredNominationAuditRule> Rules)
{
    public List<string> Validate(TieredNominationAudit item)
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

    public void RequireValid(TieredNominationAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredNominationAudit> items)
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

/// <summary>Functions of tiered nomination audit rules.</summary>
public static class TieredNominationAuditRules
{
    public static TieredNominationAuditValidator DefaultTieredNominationAuditValidator()
    {
        return new TieredNominationAuditValidator(
            new List<ITieredNominationAuditRule>
            {
                new TieredNominationAuditRequiredCodeRule("tiered nomination audit needs an entry"),
                new TieredNominationAuditFloorValueRangeRule(8.4, 529.9),
                new TieredNominationAuditRevisionRangeRule(32, 1000),
                new TieredNominationAuditTitleLengthRule(120),
                new TieredNominationAuditMinimumRangeRule(0.3, 1349.7),
                new TieredNominationAuditRequiredPhoneRule("tiered nomination audit needs a value"),
            });
    }
}
