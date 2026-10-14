namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg snapshot must pass. Check returns its problems, or none.</summary>
public interface IReorgSnapshotRule
{
    List<string> Check(ReorgSnapshot item);
}

public sealed record ReorgSnapshotRequiredSkillRule(string Message) : IReorgSnapshotRule
{
    public List<string> Check(ReorgSnapshot item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgSnapshotCodePrefixRule(string Prefix) : IReorgSnapshotRule
{
    public List<string> Check(ReorgSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgSnapshotAllowedRegionRule(List<string> Allowed) : IReorgSnapshotRule
{
    public List<string> Check(ReorgSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record ReorgSnapshotSkillLengthRule(int MaxLength) : IReorgSnapshotRule
{
    public List<string> Check(ReorgSnapshot item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgSnapshotMarginRangeRule(double Min, double Max) : IReorgSnapshotRule
{
    public List<string> Check(ReorgSnapshot item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a reorg snapshot.</summary>
public sealed record ReorgSnapshotValidator(List<IReorgSnapshotRule> Rules)
{
    public List<string> Validate(ReorgSnapshot item)
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

    public void RequireValid(ReorgSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgSnapshot> items)
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

/// <summary>Functions of reorg snapshot rules.</summary>
public static class ReorgSnapshotRules
{
    public static ReorgSnapshotValidator DefaultReorgSnapshotValidator()
    {
        return new ReorgSnapshotValidator(
            new List<IReorgSnapshotRule>
            {
                new ReorgSnapshotRequiredSkillRule("reorg snapshot needs a value"),
                new ReorgSnapshotCodePrefixRule("OPS"),
                new ReorgSnapshotAllowedRegionRule(new List<string> { "legacy" }),
                new ReorgSnapshotSkillLengthRule(120),
                new ReorgSnapshotMarginRangeRule(9.3, 4557.3),
            });
    }
}
