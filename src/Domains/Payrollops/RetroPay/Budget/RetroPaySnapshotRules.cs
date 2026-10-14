namespace Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>A rule a retro pay snapshot must pass. Check returns its problems, or none.</summary>
public interface IRetroPaySnapshotRule
{
    List<string> Check(RetroPaySnapshot item);
}

public sealed record RetroPaySnapshotCodePrefixRule(string Prefix) : IRetroPaySnapshotRule
{
    public List<string> Check(RetroPaySnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySnapshotLocationLengthRule(int MaxLength) : IRetroPaySnapshotRule
{
    public List<string> Check(RetroPaySnapshot item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySnapshotRequiredLocationRule(string Message) : IRetroPaySnapshotRule
{
    public List<string> Check(RetroPaySnapshot item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySnapshotRequiredSummaryRule(string Message) : IRetroPaySnapshotRule
{
    public List<string> Check(RetroPaySnapshot item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySnapshotLabelLengthRule(int MaxLength) : IRetroPaySnapshotRule
{
    public List<string> Check(RetroPaySnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay snapshot.</summary>
public sealed record RetroPaySnapshotValidator(List<IRetroPaySnapshotRule> Rules)
{
    public List<string> Validate(RetroPaySnapshot item)
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

    public void RequireValid(RetroPaySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPaySnapshot> items)
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

/// <summary>Functions of retro pay snapshot rules.</summary>
public static class RetroPaySnapshotRules
{
    public static RetroPaySnapshotValidator DefaultRetroPaySnapshotValidator()
    {
        return new RetroPaySnapshotValidator(
            new List<IRetroPaySnapshotRule>
            {
                new RetroPaySnapshotCodePrefixRule("REF"),
                new RetroPaySnapshotLocationLengthRule(3600),
                new RetroPaySnapshotRequiredLocationRule("retro pay snapshot needs an entry"),
                new RetroPaySnapshotRequiredSummaryRule("retro pay snapshot needs a value"),
                new RetroPaySnapshotLabelLengthRule(120),
            });
    }
}
