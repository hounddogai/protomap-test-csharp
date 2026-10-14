namespace Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>A rule a manual equity grant group must pass. Check returns its problems, or none.</summary>
public interface IManualEquityGrantGroupRule
{
    List<string> Check(ManualEquityGrantGroup item);
}

public sealed record ManualEquityGrantGroupHoursRangeRule(double Min, double Max) : IManualEquityGrantGroupRule
{
    public List<string> Check(ManualEquityGrantGroup item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualEquityGrantGroupRequiredCodeRule(string Message) : IManualEquityGrantGroupRule
{
    public List<string> Check(ManualEquityGrantGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualEquityGrantGroupRequiredTitleRule(string Message) : IManualEquityGrantGroupRule
{
    public List<string> Check(ManualEquityGrantGroup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualEquityGrantGroupCodePrefixRule(string Prefix) : IManualEquityGrantGroupRule
{
    public List<string> Check(ManualEquityGrantGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual equity grant group.</summary>
public sealed record ManualEquityGrantGroupValidator(List<IManualEquityGrantGroupRule> Rules)
{
    public List<string> Validate(ManualEquityGrantGroup item)
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

    public void RequireValid(ManualEquityGrantGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualEquityGrantGroup> items)
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

/// <summary>Functions of manual equity grant group rules.</summary>
public static class ManualEquityGrantGroupRules
{
    public static ManualEquityGrantGroupValidator DefaultManualEquityGrantGroupValidator()
    {
        return new ManualEquityGrantGroupValidator(
            new List<IManualEquityGrantGroupRule>
            {
                new ManualEquityGrantGroupHoursRangeRule(2.5, 1481.5),
                new ManualEquityGrantGroupRequiredCodeRule("manual equity grant group needs this field"),
                new ManualEquityGrantGroupRequiredTitleRule("manual equity grant group needs this field"),
                new ManualEquityGrantGroupCodePrefixRule("X"),
            });
    }
}
