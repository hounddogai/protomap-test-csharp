namespace Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>A rule a seasonal booking checkpoint must pass. Check returns its problems, or none.</summary>
public interface ISeasonalBookingCheckpointRule
{
    List<string> Check(SeasonalBookingCheckpoint item);
}

public sealed record SeasonalBookingCheckpointLeaveDaysRangeRule(int Min, int Max) : ISeasonalBookingCheckpointRule
{
    public List<string> Check(SeasonalBookingCheckpoint item)
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

public sealed record SeasonalBookingCheckpointRequiredCodeRule(string Message) : ISeasonalBookingCheckpointRule
{
    public List<string> Check(SeasonalBookingCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBookingCheckpointLabelLengthRule(int MaxLength) : ISeasonalBookingCheckpointRule
{
    public List<string> Check(SeasonalBookingCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal booking checkpoint.</summary>
public sealed record SeasonalBookingCheckpointValidator(List<ISeasonalBookingCheckpointRule> Rules)
{
    public List<string> Validate(SeasonalBookingCheckpoint item)
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

    public void RequireValid(SeasonalBookingCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalBookingCheckpoint> items)
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

/// <summary>Functions of seasonal booking checkpoint rules.</summary>
public static class SeasonalBookingCheckpointRules
{
    public static SeasonalBookingCheckpointValidator DefaultSeasonalBookingCheckpointValidator()
    {
        return new SeasonalBookingCheckpointValidator(
            new List<ISeasonalBookingCheckpointRule>
            {
                new SeasonalBookingCheckpointLeaveDaysRangeRule(1000, 86400),
                new SeasonalBookingCheckpointRequiredCodeRule("seasonal booking checkpoint needs this field"),
                new SeasonalBookingCheckpointLabelLengthRule(1000),
            });
    }
}
