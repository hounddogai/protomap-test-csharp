namespace Acme.Workforce.Absence.Balance.Policy;

/// <summary>A rule a manual balance cycle must pass. Check returns its problems, or none.</summary>
public interface IManualBalanceCycleRule
{
    List<string> Check(ManualBalanceCycle item);
}

public sealed record ManualBalanceCycleWindowRule(int MaxSpan) : IManualBalanceCycleRule
{
    public List<string> Check(ManualBalanceCycle item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceCycleNotesLengthRule(int MaxLength) : IManualBalanceCycleRule
{
    public List<string> Check(ManualBalanceCycle item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceCycleCodePrefixRule(string Prefix) : IManualBalanceCycleRule
{
    public List<string> Check(ManualBalanceCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceCycleUnitLengthRule(int MaxLength) : IManualBalanceCycleRule
{
    public List<string> Check(ManualBalanceCycle item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceCycleRequiredNameRule(string Message) : IManualBalanceCycleRule
{
    public List<string> Check(ManualBalanceCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual balance cycle.</summary>
public sealed record ManualBalanceCycleValidator(List<IManualBalanceCycleRule> Rules)
{
    public List<string> Validate(ManualBalanceCycle item)
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

    public void RequireValid(ManualBalanceCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBalanceCycle> items)
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

/// <summary>Functions of manual balance cycle rules.</summary>
public static class ManualBalanceCycleRules
{
    public static ManualBalanceCycleValidator DefaultManualBalanceCycleValidator()
    {
        return new ManualBalanceCycleValidator(
            new List<IManualBalanceCycleRule>
            {
                new ManualBalanceCycleWindowRule(3600),
                new ManualBalanceCycleNotesLengthRule(1000),
                new ManualBalanceCycleCodePrefixRule("REF"),
                new ManualBalanceCycleUnitLengthRule(64),
                new ManualBalanceCycleRequiredNameRule("manual balance cycle needs a value"),
            });
    }
}
