namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending reorg snapshot must pass. Check returns its problems, or none.</summary>
public interface IPendingReorgSnapshotRule
{
    List<string> Check(PendingReorgSnapshot item);
}

public sealed record PendingReorgSnapshotLabelLengthRule(int MaxLength) : IPendingReorgSnapshotRule
{
    public List<string> Check(PendingReorgSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgSnapshotCeilingRangeRule(double Min, double Max) : IPendingReorgSnapshotRule
{
    public List<string> Check(PendingReorgSnapshot item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgSnapshotAllowedCodeRule(List<string> Allowed) : IPendingReorgSnapshotRule
{
    public List<string> Check(PendingReorgSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgSnapshotCodeLengthRule(int MaxLength) : IPendingReorgSnapshotRule
{
    public List<string> Check(PendingReorgSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgSnapshotRequiredNameRule(string Message) : IPendingReorgSnapshotRule
{
    public List<string> Check(PendingReorgSnapshot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending reorg snapshot.</summary>
public sealed record PendingReorgSnapshotValidator(List<IPendingReorgSnapshotRule> Rules)
{
    public List<string> Validate(PendingReorgSnapshot item)
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

    public void RequireValid(PendingReorgSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingReorgSnapshot> items)
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

/// <summary>Functions of pending reorg snapshot rules.</summary>
public static class PendingReorgSnapshotRules
{
    public static PendingReorgSnapshotValidator DefaultPendingReorgSnapshotValidator()
    {
        return new PendingReorgSnapshotValidator(
            new List<IPendingReorgSnapshotRule>
            {
                new PendingReorgSnapshotLabelLengthRule(1000),
                new PendingReorgSnapshotCeilingRangeRule(0.6, 2033.1),
                new PendingReorgSnapshotAllowedCodeRule(new List<string> { "primary" }),
                new PendingReorgSnapshotCodeLengthRule(120),
                new PendingReorgSnapshotRequiredNameRule("pending reorg snapshot needs an entry"),
            });
    }
}
