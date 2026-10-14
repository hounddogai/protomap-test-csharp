namespace Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>A rule a pending skill profile target must pass. Check returns its problems, or none.</summary>
public interface IPendingSkillProfileTargetRule
{
    List<string> Check(PendingSkillProfileTarget item);
}

public sealed record PendingSkillProfileTargetCodeLengthRule(int MaxLength) : IPendingSkillProfileTargetRule
{
    public List<string> Check(PendingSkillProfileTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileTargetTimezoneLengthRule(int MaxLength) : IPendingSkillProfileTargetRule
{
    public List<string> Check(PendingSkillProfileTarget item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileTargetRequiredCodeRule(string Message) : IPendingSkillProfileTargetRule
{
    public List<string> Check(PendingSkillProfileTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileTargetRequiredNameRule(string Message) : IPendingSkillProfileTargetRule
{
    public List<string> Check(PendingSkillProfileTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending skill profile target.</summary>
public sealed record PendingSkillProfileTargetValidator(List<IPendingSkillProfileTargetRule> Rules)
{
    public List<string> Validate(PendingSkillProfileTarget item)
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

    public void RequireValid(PendingSkillProfileTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSkillProfileTarget> items)
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

/// <summary>Functions of pending skill profile target rules.</summary>
public static class PendingSkillProfileTargetRules
{
    public static PendingSkillProfileTargetValidator DefaultPendingSkillProfileTargetValidator()
    {
        return new PendingSkillProfileTargetValidator(
            new List<IPendingSkillProfileTargetRule>
            {
                new PendingSkillProfileTargetCodeLengthRule(64),
                new PendingSkillProfileTargetTimezoneLengthRule(80),
                new PendingSkillProfileTargetRequiredCodeRule("pending skill profile target needs an entry"),
                new PendingSkillProfileTargetRequiredNameRule("pending skill profile target needs an entry"),
            });
    }
}
