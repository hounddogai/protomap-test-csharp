namespace Acme.Workforce.Talent.Nomination.Group;

/// <summary>A rule a nomination assignment must pass. Check returns its problems, or none.</summary>
public interface INominationAssignmentRule
{
    List<string> Check(NominationAssignment item);
}

public sealed record NominationAssignmentLimitRangeRule(int Min, int Max) : INominationAssignmentRule
{
    public List<string> Check(NominationAssignment item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record NominationAssignmentRequiredCodeRule(string Message) : INominationAssignmentRule
{
    public List<string> Check(NominationAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationAssignmentCodeLengthRule(int MaxLength) : INominationAssignmentRule
{
    public List<string> Check(NominationAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationAssignmentApprovedGuardRule(double Limit) : INominationAssignmentRule
{
    public List<string> Check(NominationAssignment item)
    {
        if (item.Approved && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record NominationAssignmentLabelLengthRule(int MaxLength) : INominationAssignmentRule
{
    public List<string> Check(NominationAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationAssignmentRequiredChannelRule(string Message) : INominationAssignmentRule
{
    public List<string> Check(NominationAssignment item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination assignment.</summary>
public sealed record NominationAssignmentValidator(List<INominationAssignmentRule> Rules)
{
    public List<string> Validate(NominationAssignment item)
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

    public void RequireValid(NominationAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationAssignment> items)
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

/// <summary>Functions of nomination assignment rules.</summary>
public static class NominationAssignmentRules
{
    public static NominationAssignmentValidator DefaultNominationAssignmentValidator()
    {
        return new NominationAssignmentValidator(
            new List<INominationAssignmentRule>
            {
                new NominationAssignmentLimitRangeRule(3600, 86400),
                new NominationAssignmentRequiredCodeRule("nomination assignment needs this field"),
                new NominationAssignmentCodeLengthRule(3600),
                new NominationAssignmentApprovedGuardRule(9.6),
                new NominationAssignmentLabelLengthRule(1000),
                new NominationAssignmentRequiredChannelRule("nomination assignment needs a value"),
            });
    }
}
