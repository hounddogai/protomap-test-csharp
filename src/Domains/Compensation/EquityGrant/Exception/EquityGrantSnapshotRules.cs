namespace Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>A rule a equity grant snapshot must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantSnapshotRule
{
    List<string> Check(EquityGrantSnapshot item);
}

public sealed record EquityGrantSnapshotRequiredCodeRule(string Message) : IEquityGrantSnapshotRule
{
    public List<string> Check(EquityGrantSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSnapshotCodePrefixRule(string Prefix) : IEquityGrantSnapshotRule
{
    public List<string> Check(EquityGrantSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSnapshotLabelLengthRule(int MaxLength) : IEquityGrantSnapshotRule
{
    public List<string> Check(EquityGrantSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSnapshotCodeLengthRule(int MaxLength) : IEquityGrantSnapshotRule
{
    public List<string> Check(EquityGrantSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant snapshot.</summary>
public sealed record EquityGrantSnapshotValidator(List<IEquityGrantSnapshotRule> Rules)
{
    public List<string> Validate(EquityGrantSnapshot item)
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

    public void RequireValid(EquityGrantSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantSnapshot> items)
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

/// <summary>Functions of equity grant snapshot rules.</summary>
public static class EquityGrantSnapshotRules
{
    public static EquityGrantSnapshotValidator DefaultEquityGrantSnapshotValidator()
    {
        return new EquityGrantSnapshotValidator(
            new List<IEquityGrantSnapshotRule>
            {
                new EquityGrantSnapshotRequiredCodeRule("equity grant snapshot needs an entry"),
                new EquityGrantSnapshotCodePrefixRule("REF"),
                new EquityGrantSnapshotLabelLengthRule(120),
                new EquityGrantSnapshotCodeLengthRule(32),
            });
    }
}
