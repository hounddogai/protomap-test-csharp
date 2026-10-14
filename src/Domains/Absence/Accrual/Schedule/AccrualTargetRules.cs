namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual target must pass. Check returns its problems, or none.</summary>
public interface IAccrualTargetRule
{
    List<string> Check(AccrualTarget item);
}

public sealed record AccrualTargetRequiredChannelRule(string Message) : IAccrualTargetRule
{
    public List<string> Check(AccrualTarget item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualTargetRequiredNameRule(string Message) : IAccrualTargetRule
{
    public List<string> Check(AccrualTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualTargetRetriesRangeRule(int Min, int Max) : IAccrualTargetRule
{
    public List<string> Check(AccrualTarget item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccrualTargetCreatedAtRangeRule(int Min, int Max) : IAccrualTargetRule
{
    public List<string> Check(AccrualTarget item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccrualTargetRequiredCodeRule(string Message) : IAccrualTargetRule
{
    public List<string> Check(AccrualTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualTargetAllowedChannelRule(List<string> Allowed) : IAccrualTargetRule
{
    public List<string> Check(AccrualTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual target.</summary>
public sealed record AccrualTargetValidator(List<IAccrualTargetRule> Rules)
{
    public List<string> Validate(AccrualTarget item)
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

    public void RequireValid(AccrualTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualTarget> items)
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

/// <summary>Functions of accrual target rules.</summary>
public static class AccrualTargetRules
{
    public static AccrualTargetValidator DefaultAccrualTargetValidator()
    {
        return new AccrualTargetValidator(
            new List<IAccrualTargetRule>
            {
                new AccrualTargetRequiredChannelRule("accrual target needs a value"),
                new AccrualTargetRequiredNameRule("accrual target needs this field"),
                new AccrualTargetRetriesRangeRule(120, 255),
                new AccrualTargetCreatedAtRangeRule(120, 3600),
                new AccrualTargetRequiredCodeRule("accrual target needs a value"),
                new AccrualTargetAllowedChannelRule(new List<string> { "strict" }),
            });
    }
}
