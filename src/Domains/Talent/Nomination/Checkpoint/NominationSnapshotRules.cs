namespace Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>A rule a nomination snapshot must pass. Check returns its problems, or none.</summary>
public interface INominationSnapshotRule
{
    List<string> Check(NominationSnapshot item);
}

public sealed record NominationSnapshotBadgeLengthRule(int MaxLength) : INominationSnapshotRule
{
    public List<string> Check(NominationSnapshot item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationSnapshotRequiredBadgeRule(string Message) : INominationSnapshotRule
{
    public List<string> Check(NominationSnapshot item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationSnapshotCodeLengthRule(int MaxLength) : INominationSnapshotRule
{
    public List<string> Check(NominationSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationSnapshotNameLengthRule(int MaxLength) : INominationSnapshotRule
{
    public List<string> Check(NominationSnapshot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination snapshot.</summary>
public sealed record NominationSnapshotValidator(List<INominationSnapshotRule> Rules)
{
    public List<string> Validate(NominationSnapshot item)
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

    public void RequireValid(NominationSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationSnapshot> items)
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

/// <summary>Functions of nomination snapshot rules.</summary>
public static class NominationSnapshotRules
{
    public static NominationSnapshotValidator DefaultNominationSnapshotValidator()
    {
        return new NominationSnapshotValidator(
            new List<INominationSnapshotRule>
            {
                new NominationSnapshotBadgeLengthRule(64),
                new NominationSnapshotRequiredBadgeRule("nomination snapshot needs an entry"),
                new NominationSnapshotCodeLengthRule(1000),
                new NominationSnapshotNameLengthRule(64),
            });
    }
}
