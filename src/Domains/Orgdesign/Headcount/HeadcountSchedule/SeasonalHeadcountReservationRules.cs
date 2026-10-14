namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>A rule a seasonal headcount reservation must pass. Check returns its problems, or none.</summary>
public interface ISeasonalHeadcountReservationRule
{
    List<string> Check(SeasonalHeadcountReservation item);
}

public sealed record SeasonalHeadcountReservationRequiredDepartmentRule(string Message) : ISeasonalHeadcountReservationRule
{
    public List<string> Check(SeasonalHeadcountReservation item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHeadcountReservationCodeLengthRule(int MaxLength) : ISeasonalHeadcountReservationRule
{
    public List<string> Check(SeasonalHeadcountReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHeadcountReservationRequiredSkillRule(string Message) : ISeasonalHeadcountReservationRule
{
    public List<string> Check(SeasonalHeadcountReservation item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal headcount reservation.</summary>
public sealed record SeasonalHeadcountReservationValidator(List<ISeasonalHeadcountReservationRule> Rules)
{
    public List<string> Validate(SeasonalHeadcountReservation item)
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

    public void RequireValid(SeasonalHeadcountReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalHeadcountReservation> items)
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

/// <summary>Functions of seasonal headcount reservation rules.</summary>
public static class SeasonalHeadcountReservationRules
{
    public static SeasonalHeadcountReservationValidator DefaultSeasonalHeadcountReservationValidator()
    {
        return new SeasonalHeadcountReservationValidator(
            new List<ISeasonalHeadcountReservationRule>
            {
                new SeasonalHeadcountReservationRequiredDepartmentRule(
                    "seasonal headcount reservation needs an entry"),
                new SeasonalHeadcountReservationCodeLengthRule(16),
                new SeasonalHeadcountReservationRequiredSkillRule("seasonal headcount reservation needs a value"),
            });
    }
}
