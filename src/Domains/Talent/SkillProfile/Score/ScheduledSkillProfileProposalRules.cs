namespace Acme.Workforce.Talent.SkillProfile.Score;

/// <summary>A rule a scheduled skill profile proposal must pass. Check returns its problems, or none.</summary>
public interface IScheduledSkillProfileProposalRule
{
    List<string> Check(ScheduledSkillProfileProposal item);
}

public sealed record ScheduledSkillProfileProposalPriorityRangeRule(int Min, int Max) : IScheduledSkillProfileProposalRule
{
    public List<string> Check(ScheduledSkillProfileProposal item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSkillProfileProposalRequiredEmployeeIdRule(string Message) : IScheduledSkillProfileProposalRule
{
    public List<string> Check(ScheduledSkillProfileProposal item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSkillProfileProposalContactEmailLengthRule(int MaxLength) : IScheduledSkillProfileProposalRule
{
    public List<string> Check(ScheduledSkillProfileProposal item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSkillProfileProposalRequiredContactEmailRule(string Message) : IScheduledSkillProfileProposalRule
{
    public List<string> Check(ScheduledSkillProfileProposal item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSkillProfileProposalMaximumRangeRule(double Min, double Max) : IScheduledSkillProfileProposalRule
{
    public List<string> Check(ScheduledSkillProfileProposal item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled skill profile proposal.</summary>
public sealed record ScheduledSkillProfileProposalValidator(List<IScheduledSkillProfileProposalRule> Rules)
{
    public List<string> Validate(ScheduledSkillProfileProposal item)
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

    public void RequireValid(ScheduledSkillProfileProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledSkillProfileProposal> items)
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

/// <summary>Functions of scheduled skill profile proposal rules.</summary>
public static class ScheduledSkillProfileProposalRules
{
    public static ScheduledSkillProfileProposalValidator DefaultScheduledSkillProfileProposalValidator()
    {
        return new ScheduledSkillProfileProposalValidator(
            new List<IScheduledSkillProfileProposalRule>
            {
                new ScheduledSkillProfileProposalPriorityRangeRule(120, 255),
                new ScheduledSkillProfileProposalRequiredEmployeeIdRule(
                    "scheduled skill profile proposal needs this field"),
                new ScheduledSkillProfileProposalContactEmailLengthRule(80),
                new ScheduledSkillProfileProposalRequiredContactEmailRule(
                    "scheduled skill profile proposal needs a value"),
                new ScheduledSkillProfileProposalMaximumRangeRule(9.5, 1593.7),
            });
    }
}
