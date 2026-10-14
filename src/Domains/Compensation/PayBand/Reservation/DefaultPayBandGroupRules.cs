namespace Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>A rule a default pay band group must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayBandGroupRule
{
    List<string> Check(DefaultPayBandGroup item);
}

public sealed record DefaultPayBandGroupRequiredTitleRule(string Message) : IDefaultPayBandGroupRule
{
    public List<string> Check(DefaultPayBandGroup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayBandGroupRetriesRangeRule(int Min, int Max) : IDefaultPayBandGroupRule
{
    public List<string> Check(DefaultPayBandGroup item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayBandGroupCodePrefixRule(string Prefix) : IDefaultPayBandGroupRule
{
    public List<string> Check(DefaultPayBandGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayBandGroupTenureMonthsRangeRule(int Min, int Max) : IDefaultPayBandGroupRule
{
    public List<string> Check(DefaultPayBandGroup item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default pay band group.</summary>
public sealed record DefaultPayBandGroupValidator(List<IDefaultPayBandGroupRule> Rules)
{
    public List<string> Validate(DefaultPayBandGroup item)
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

    public void RequireValid(DefaultPayBandGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayBandGroup> items)
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

/// <summary>Functions of default pay band group rules.</summary>
public static class DefaultPayBandGroupRules
{
    public static DefaultPayBandGroupValidator DefaultDefaultPayBandGroupValidator()
    {
        return new DefaultPayBandGroupValidator(
            new List<IDefaultPayBandGroupRule>
            {
                new DefaultPayBandGroupRequiredTitleRule("default pay band group needs this field"),
                new DefaultPayBandGroupRetriesRangeRule(64, 64),
                new DefaultPayBandGroupCodePrefixRule("X"),
                new DefaultPayBandGroupTenureMonthsRangeRule(64, 1000),
            });
    }
}
