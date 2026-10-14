namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount snapshot must pass. Check returns its problems, or none.</summary>
public interface IHeadcountSnapshotRule
{
    List<string> Check(HeadcountSnapshot item);
}

public sealed record HeadcountSnapshotAllowedLabelRule(List<string> Allowed) : IHeadcountSnapshotRule
{
    public List<string> Check(HeadcountSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is too long" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountSnapshotVersionRangeRule(int Min, int Max) : IHeadcountSnapshotRule
{
    public List<string> Check(HeadcountSnapshot item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountSnapshotScoreRangeRule(double Min, double Max) : IHeadcountSnapshotRule
{
    public List<string> Check(HeadcountSnapshot item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountSnapshotCodePrefixRule(string Prefix) : IHeadcountSnapshotRule
{
    public List<string> Check(HeadcountSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountSnapshotLabelLengthRule(int MaxLength) : IHeadcountSnapshotRule
{
    public List<string> Check(HeadcountSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount snapshot.</summary>
public sealed record HeadcountSnapshotValidator(List<IHeadcountSnapshotRule> Rules)
{
    public List<string> Validate(HeadcountSnapshot item)
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

    public void RequireValid(HeadcountSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountSnapshot> items)
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

/// <summary>Functions of headcount snapshot rules.</summary>
public static class HeadcountSnapshotRules
{
    public static HeadcountSnapshotValidator DefaultHeadcountSnapshotValidator()
    {
        return new HeadcountSnapshotValidator(
            new List<IHeadcountSnapshotRule>
            {
                new HeadcountSnapshotAllowedLabelRule(new List<string> { "rolling", "adaptive", "capped" }),
                new HeadcountSnapshotVersionRangeRule(16, 3600),
                new HeadcountSnapshotScoreRangeRule(6.9, 1327.7),
                new HeadcountSnapshotCodePrefixRule("REF"),
                new HeadcountSnapshotLabelLengthRule(120),
            });
    }
}
