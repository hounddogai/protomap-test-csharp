namespace Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>A rule a pending skill profile digest must pass. Check returns its problems, or none.</summary>
public interface IPendingSkillProfileDigestRule
{
    List<string> Check(PendingSkillProfileDigest item);
}

public sealed record PendingSkillProfileDigestRequiredNameRule(string Message) : IPendingSkillProfileDigestRule
{
    public List<string> Check(PendingSkillProfileDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileDigestCodeLengthRule(int MaxLength) : IPendingSkillProfileDigestRule
{
    public List<string> Check(PendingSkillProfileDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileDigestSequenceRangeRule(int Min, int Max) : IPendingSkillProfileDigestRule
{
    public List<string> Check(PendingSkillProfileDigest item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingSkillProfileDigestRequiredEmployeeIdRule(string Message) : IPendingSkillProfileDigestRule
{
    public List<string> Check(PendingSkillProfileDigest item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileDigestEmployeeIdLengthRule(int MaxLength) : IPendingSkillProfileDigestRule
{
    public List<string> Check(PendingSkillProfileDigest item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending skill profile digest.</summary>
public sealed record PendingSkillProfileDigestValidator(List<IPendingSkillProfileDigestRule> Rules)
{
    public List<string> Validate(PendingSkillProfileDigest item)
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

    public void RequireValid(PendingSkillProfileDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSkillProfileDigest> items)
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

/// <summary>Functions of pending skill profile digest rules.</summary>
public static class PendingSkillProfileDigestRules
{
    public static PendingSkillProfileDigestValidator DefaultPendingSkillProfileDigestValidator()
    {
        return new PendingSkillProfileDigestValidator(
            new List<IPendingSkillProfileDigestRule>
            {
                new PendingSkillProfileDigestRequiredNameRule("pending skill profile digest needs a value"),
                new PendingSkillProfileDigestCodeLengthRule(255),
                new PendingSkillProfileDigestSequenceRangeRule(80, 3600),
                new PendingSkillProfileDigestRequiredEmployeeIdRule("pending skill profile digest needs this field"),
                new PendingSkillProfileDigestEmployeeIdLengthRule(32),
            });
    }
}
