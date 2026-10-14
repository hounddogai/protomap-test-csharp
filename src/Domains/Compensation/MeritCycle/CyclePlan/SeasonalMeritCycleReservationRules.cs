namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>A rule a seasonal merit cycle reservation must pass. Check returns its problems, or none.</summary>
public interface ISeasonalMeritCycleReservationRule
{
    List<string> Check(SeasonalMeritCycleReservation item);
}

public sealed record SeasonalMeritCycleReservationThresholdRangeRule(double Min, double Max) : ISeasonalMeritCycleReservationRule
{
    public List<string> Check(SeasonalMeritCycleReservation item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMeritCycleReservationLeaveDaysRangeRule(int Min, int Max) : ISeasonalMeritCycleReservationRule
{
    public List<string> Check(SeasonalMeritCycleReservation item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalMeritCycleReservationRequiredNotesRule(string Message) : ISeasonalMeritCycleReservationRule
{
    public List<string> Check(SeasonalMeritCycleReservation item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMeritCycleReservationRequiredGradeRule(string Message) : ISeasonalMeritCycleReservationRule
{
    public List<string> Check(SeasonalMeritCycleReservation item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMeritCycleReservationNotesLengthRule(int MaxLength) : ISeasonalMeritCycleReservationRule
{
    public List<string> Check(SeasonalMeritCycleReservation item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal merit cycle reservation.</summary>
public sealed record SeasonalMeritCycleReservationValidator(List<ISeasonalMeritCycleReservationRule> Rules)
{
    public List<string> Validate(SeasonalMeritCycleReservation item)
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

    public void RequireValid(SeasonalMeritCycleReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalMeritCycleReservation> items)
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

/// <summary>Functions of seasonal merit cycle reservation rules.</summary>
public static class SeasonalMeritCycleReservationRules
{
    public static SeasonalMeritCycleReservationValidator DefaultSeasonalMeritCycleReservationValidator()
    {
        return new SeasonalMeritCycleReservationValidator(
            new List<ISeasonalMeritCycleReservationRule>
            {
                new SeasonalMeritCycleReservationThresholdRangeRule(6.2, 4942.1),
                new SeasonalMeritCycleReservationLeaveDaysRangeRule(1000, 3600),
                new SeasonalMeritCycleReservationRequiredNotesRule(
                    "seasonal merit cycle reservation needs this field"),
                new SeasonalMeritCycleReservationRequiredGradeRule("seasonal merit cycle reservation needs an entry"),
                new SeasonalMeritCycleReservationNotesLengthRule(16),
            });
    }
}
