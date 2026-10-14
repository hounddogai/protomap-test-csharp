namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>A rule a skill profile snapshot must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileSnapshotRule
{
    List<string> Check(SkillProfileSnapshot item);
}

public sealed record SkillProfileSnapshotCodeLengthRule(int MaxLength) : ISkillProfileSnapshotRule
{
    public List<string> Check(SkillProfileSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSnapshotMinimumRangeRule(double Min, double Max) : ISkillProfileSnapshotRule
{
    public List<string> Check(SkillProfileSnapshot item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSnapshotRequiredCodeRule(string Message) : ISkillProfileSnapshotRule
{
    public List<string> Check(SkillProfileSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSnapshotRequiredJobCodeRule(string Message) : ISkillProfileSnapshotRule
{
    public List<string> Check(SkillProfileSnapshot item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSnapshotLabelLengthRule(int MaxLength) : ISkillProfileSnapshotRule
{
    public List<string> Check(SkillProfileSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile snapshot.</summary>
public sealed record SkillProfileSnapshotValidator(List<ISkillProfileSnapshotRule> Rules)
{
    public List<string> Validate(SkillProfileSnapshot item)
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

    public void RequireValid(SkillProfileSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileSnapshot> items)
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

/// <summary>Functions of skill profile snapshot rules.</summary>
public static class SkillProfileSnapshotRules
{
    public static SkillProfileSnapshotValidator DefaultSkillProfileSnapshotValidator()
    {
        return new SkillProfileSnapshotValidator(
            new List<ISkillProfileSnapshotRule>
            {
                new SkillProfileSnapshotCodeLengthRule(86400),
                new SkillProfileSnapshotMinimumRangeRule(2.5, 1459.5),
                new SkillProfileSnapshotRequiredCodeRule("skill profile snapshot needs an entry"),
                new SkillProfileSnapshotRequiredJobCodeRule("skill profile snapshot needs this field"),
                new SkillProfileSnapshotLabelLengthRule(80),
            });
    }
}
