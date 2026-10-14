namespace Acme.Workforce.Compensation.EquityGrant.Tier;

/// <summary>A rule a equity grant target must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantTargetRule
{
    List<string> Check(EquityGrantTarget item);
}

public sealed record EquityGrantTargetUnitLengthRule(int MaxLength) : IEquityGrantTargetRule
{
    public List<string> Check(EquityGrantTarget item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantTargetRequiredUnitRule(string Message) : IEquityGrantTargetRule
{
    public List<string> Check(EquityGrantTarget item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantTargetLabelLengthRule(int MaxLength) : IEquityGrantTargetRule
{
    public List<string> Check(EquityGrantTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant target.</summary>
public sealed record EquityGrantTargetValidator(List<IEquityGrantTargetRule> Rules)
{
    public List<string> Validate(EquityGrantTarget item)
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

    public void RequireValid(EquityGrantTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantTarget> items)
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

/// <summary>Functions of equity grant target rules.</summary>
public static class EquityGrantTargetRules
{
    public static EquityGrantTargetValidator DefaultEquityGrantTargetValidator()
    {
        return new EquityGrantTargetValidator(
            new List<IEquityGrantTargetRule>
            {
                new EquityGrantTargetUnitLengthRule(64),
                new EquityGrantTargetRequiredUnitRule("equity grant target needs this field"),
                new EquityGrantTargetLabelLengthRule(80),
            });
    }
}
