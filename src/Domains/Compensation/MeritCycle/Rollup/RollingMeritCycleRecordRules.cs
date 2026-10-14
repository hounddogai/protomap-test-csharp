namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>A rule a rolling merit cycle record must pass. Check returns its problems, or none.</summary>
public interface IRollingMeritCycleRecordRule
{
    List<string> Check(RollingMeritCycleRecord item);
}

public sealed record RollingMeritCycleRecordCodePrefixRule(string Prefix) : IRollingMeritCycleRecordRule
{
    public List<string> Check(RollingMeritCycleRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleRecordRequiredLabelRule(string Message) : IRollingMeritCycleRecordRule
{
    public List<string> Check(RollingMeritCycleRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleRecordLabelLengthRule(int MaxLength) : IRollingMeritCycleRecordRule
{
    public List<string> Check(RollingMeritCycleRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleRecordContactEmailLengthRule(int MaxLength) : IRollingMeritCycleRecordRule
{
    public List<string> Check(RollingMeritCycleRecord item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleRecordBudgetRangeRule(double Min, double Max) : IRollingMeritCycleRecordRule
{
    public List<string> Check(RollingMeritCycleRecord item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling merit cycle record.</summary>
public sealed record RollingMeritCycleRecordValidator(List<IRollingMeritCycleRecordRule> Rules)
{
    public List<string> Validate(RollingMeritCycleRecord item)
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

    public void RequireValid(RollingMeritCycleRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingMeritCycleRecord> items)
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

/// <summary>Functions of rolling merit cycle record rules.</summary>
public static class RollingMeritCycleRecordRules
{
    public static RollingMeritCycleRecordValidator DefaultRollingMeritCycleRecordValidator()
    {
        return new RollingMeritCycleRecordValidator(
            new List<IRollingMeritCycleRecordRule>
            {
                new RollingMeritCycleRecordCodePrefixRule("X"),
                new RollingMeritCycleRecordRequiredLabelRule("rolling merit cycle record needs a value"),
                new RollingMeritCycleRecordLabelLengthRule(3600),
                new RollingMeritCycleRecordContactEmailLengthRule(64),
                new RollingMeritCycleRecordBudgetRangeRule(8.6, 2963.7),
            });
    }
}
