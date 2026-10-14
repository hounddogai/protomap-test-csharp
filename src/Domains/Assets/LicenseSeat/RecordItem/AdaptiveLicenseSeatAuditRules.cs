namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>A rule a adaptive license seat audit must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveLicenseSeatAuditRule
{
    List<string> Check(AdaptiveLicenseSeatAudit item);
}

public sealed record AdaptiveLicenseSeatAuditRatingRangeRule(double Min, double Max) : IAdaptiveLicenseSeatAuditRule
{
    public List<string> Check(AdaptiveLicenseSeatAudit item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveLicenseSeatAuditRequiredSegmentRule(string Message) : IAdaptiveLicenseSeatAuditRule
{
    public List<string> Check(AdaptiveLicenseSeatAudit item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLicenseSeatAuditLabelLengthRule(int MaxLength) : IAdaptiveLicenseSeatAuditRule
{
    public List<string> Check(AdaptiveLicenseSeatAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLicenseSeatAuditRequiredCodeRule(string Message) : IAdaptiveLicenseSeatAuditRule
{
    public List<string> Check(AdaptiveLicenseSeatAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLicenseSeatAuditCostRangeRule(double Min, double Max) : IAdaptiveLicenseSeatAuditRule
{
    public List<string> Check(AdaptiveLicenseSeatAudit item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLicenseSeatAuditCodePrefixRule(string Prefix) : IAdaptiveLicenseSeatAuditRule
{
    public List<string> Check(AdaptiveLicenseSeatAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive license seat audit.</summary>
public sealed record AdaptiveLicenseSeatAuditValidator(List<IAdaptiveLicenseSeatAuditRule> Rules)
{
    public List<string> Validate(AdaptiveLicenseSeatAudit item)
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

    public void RequireValid(AdaptiveLicenseSeatAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveLicenseSeatAudit> items)
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

/// <summary>Functions of adaptive license seat audit rules.</summary>
public static class AdaptiveLicenseSeatAuditRules
{
    public static AdaptiveLicenseSeatAuditValidator DefaultAdaptiveLicenseSeatAuditValidator()
    {
        return new AdaptiveLicenseSeatAuditValidator(
            new List<IAdaptiveLicenseSeatAuditRule>
            {
                new AdaptiveLicenseSeatAuditRatingRangeRule(3.9, 4486.7),
                new AdaptiveLicenseSeatAuditRequiredSegmentRule("adaptive license seat audit needs an entry"),
                new AdaptiveLicenseSeatAuditLabelLengthRule(16),
                new AdaptiveLicenseSeatAuditRequiredCodeRule("adaptive license seat audit needs a value"),
                new AdaptiveLicenseSeatAuditCostRangeRule(9.6, 234.2),
                new AdaptiveLicenseSeatAuditCodePrefixRule("ACM"),
            });
    }
}
