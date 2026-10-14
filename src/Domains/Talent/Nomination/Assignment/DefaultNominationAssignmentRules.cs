namespace Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>A rule a default nomination assignment must pass. Check returns its problems, or none.</summary>
public interface IDefaultNominationAssignmentRule
{
    List<string> Check(DefaultNominationAssignment item);
}

public sealed record DefaultNominationAssignmentRequiredCodeRule(string Message) : IDefaultNominationAssignmentRule
{
    public List<string> Check(DefaultNominationAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultNominationAssignmentRequiredPhoneRule(string Message) : IDefaultNominationAssignmentRule
{
    public List<string> Check(DefaultNominationAssignment item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultNominationAssignmentTitleLengthRule(int MaxLength) : IDefaultNominationAssignmentRule
{
    public List<string> Check(DefaultNominationAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default nomination assignment.</summary>
public sealed record DefaultNominationAssignmentValidator(List<IDefaultNominationAssignmentRule> Rules)
{
    public List<string> Validate(DefaultNominationAssignment item)
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

    public void RequireValid(DefaultNominationAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultNominationAssignment> items)
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

/// <summary>Functions of default nomination assignment rules.</summary>
public static class DefaultNominationAssignmentRules
{
    public static DefaultNominationAssignmentValidator DefaultDefaultNominationAssignmentValidator()
    {
        return new DefaultNominationAssignmentValidator(
            new List<IDefaultNominationAssignmentRule>
            {
                new DefaultNominationAssignmentRequiredCodeRule("default nomination assignment needs this field"),
                new DefaultNominationAssignmentRequiredPhoneRule("default nomination assignment needs an entry"),
                new DefaultNominationAssignmentTitleLengthRule(120),
            });
    }
}
