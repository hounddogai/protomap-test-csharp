namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile reservation must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileReservationRule
{
    List<string> Check(SkillProfileReservation item);
}

public sealed record SkillProfileReservationRetriesRangeRule(int Min, int Max) : ISkillProfileReservationRule
{
    public List<string> Check(SkillProfileReservation item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileReservationRequiredTitleRule(string Message) : ISkillProfileReservationRule
{
    public List<string> Check(SkillProfileReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileReservationNotesLengthRule(int MaxLength) : ISkillProfileReservationRule
{
    public List<string> Check(SkillProfileReservation item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileReservationTitleLengthRule(int MaxLength) : ISkillProfileReservationRule
{
    public List<string> Check(SkillProfileReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileReservationAllowedNotesRule(List<string> Allowed) : ISkillProfileReservationRule
{
    public List<string> Check(SkillProfileReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile reservation.</summary>
public sealed record SkillProfileReservationValidator(List<ISkillProfileReservationRule> Rules)
{
    public List<string> Validate(SkillProfileReservation item)
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

    public void RequireValid(SkillProfileReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileReservation> items)
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

/// <summary>Functions of skill profile reservation rules.</summary>
public static class SkillProfileReservationRules
{
    public static SkillProfileReservationValidator DefaultSkillProfileReservationValidator()
    {
        return new SkillProfileReservationValidator(
            new List<ISkillProfileReservationRule>
            {
                new SkillProfileReservationRetriesRangeRule(1000, 86400),
                new SkillProfileReservationRequiredTitleRule("skill profile reservation needs a value"),
                new SkillProfileReservationNotesLengthRule(80),
                new SkillProfileReservationTitleLengthRule(16),
                new SkillProfileReservationAllowedNotesRule(new List<string> { "seasonal", "adaptive", "capped" }),
            });
    }
}
