namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped equity grant target must pass. Check returns its problems, or none.</summary>
public interface ICappedEquityGrantTargetRule
{
    List<string> Check(CappedEquityGrantTarget item);
}

public sealed record CappedEquityGrantTargetRequiredTitleRule(string Message) : ICappedEquityGrantTargetRule
{
    public List<string> Check(CappedEquityGrantTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantTargetAllowedTitleRule(List<string> Allowed) : ICappedEquityGrantTargetRule
{
    public List<string> Check(CappedEquityGrantTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantTargetEmployeeIdLengthRule(int MaxLength) : ICappedEquityGrantTargetRule
{
    public List<string> Check(CappedEquityGrantTarget item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantTargetTitleLengthRule(int MaxLength) : ICappedEquityGrantTargetRule
{
    public List<string> Check(CappedEquityGrantTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantTargetRequiredEmployeeIdRule(string Message) : ICappedEquityGrantTargetRule
{
    public List<string> Check(CappedEquityGrantTarget item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped equity grant target.</summary>
public sealed record CappedEquityGrantTargetValidator(List<ICappedEquityGrantTargetRule> Rules)
{
    public List<string> Validate(CappedEquityGrantTarget item)
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

    public void RequireValid(CappedEquityGrantTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedEquityGrantTarget> items)
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

/// <summary>Functions of capped equity grant target rules.</summary>
public static class CappedEquityGrantTargetRules
{
    public static CappedEquityGrantTargetValidator DefaultCappedEquityGrantTargetValidator()
    {
        return new CappedEquityGrantTargetValidator(
            new List<ICappedEquityGrantTargetRule>
            {
                new CappedEquityGrantTargetRequiredTitleRule("capped equity grant target needs an entry"),
                new CappedEquityGrantTargetAllowedTitleRule(new List<string> { "scheduled" }),
                new CappedEquityGrantTargetEmployeeIdLengthRule(32),
                new CappedEquityGrantTargetTitleLengthRule(120),
                new CappedEquityGrantTargetRequiredEmployeeIdRule("capped equity grant target needs an entry"),
            });
    }
}
