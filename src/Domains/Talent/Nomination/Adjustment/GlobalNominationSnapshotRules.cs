namespace Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>A rule a global nomination snapshot must pass. Check returns its problems, or none.</summary>
public interface IGlobalNominationSnapshotRule
{
    List<string> Check(GlobalNominationSnapshot item);
}

public sealed record GlobalNominationSnapshotBadgeLengthRule(int MaxLength) : IGlobalNominationSnapshotRule
{
    public List<string> Check(GlobalNominationSnapshot item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationSnapshotCodePrefixRule(string Prefix) : IGlobalNominationSnapshotRule
{
    public List<string> Check(GlobalNominationSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationSnapshotRequiredTitleRule(string Message) : IGlobalNominationSnapshotRule
{
    public List<string> Check(GlobalNominationSnapshot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationSnapshotRequiredCodeRule(string Message) : IGlobalNominationSnapshotRule
{
    public List<string> Check(GlobalNominationSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global nomination snapshot.</summary>
public sealed record GlobalNominationSnapshotValidator(List<IGlobalNominationSnapshotRule> Rules)
{
    public List<string> Validate(GlobalNominationSnapshot item)
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

    public void RequireValid(GlobalNominationSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalNominationSnapshot> items)
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

/// <summary>Functions of global nomination snapshot rules.</summary>
public static class GlobalNominationSnapshotRules
{
    public static GlobalNominationSnapshotValidator DefaultGlobalNominationSnapshotValidator()
    {
        return new GlobalNominationSnapshotValidator(
            new List<IGlobalNominationSnapshotRule>
            {
                new GlobalNominationSnapshotBadgeLengthRule(3600),
                new GlobalNominationSnapshotCodePrefixRule("X"),
                new GlobalNominationSnapshotRequiredTitleRule("global nomination snapshot needs a value"),
                new GlobalNominationSnapshotRequiredCodeRule("global nomination snapshot needs an entry"),
            });
    }
}
