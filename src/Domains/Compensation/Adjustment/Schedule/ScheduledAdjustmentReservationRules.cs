namespace Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>A rule a scheduled adjustment reservation must pass. Check returns its problems, or none.</summary>
public interface IScheduledAdjustmentReservationRule
{
    List<string> Check(ScheduledAdjustmentReservation item);
}

public sealed record ScheduledAdjustmentReservationCodeLengthRule(int MaxLength) : IScheduledAdjustmentReservationRule
{
    public List<string> Check(ScheduledAdjustmentReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentReservationCeilingRangeRule(double Min, double Max) : IScheduledAdjustmentReservationRule
{
    public List<string> Check(ScheduledAdjustmentReservation item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledAdjustmentReservationPriorityRangeRule(int Min, int Max) : IScheduledAdjustmentReservationRule
{
    public List<string> Check(ScheduledAdjustmentReservation item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentReservationGraceDaysRangeRule(int Min, int Max) : IScheduledAdjustmentReservationRule
{
    public List<string> Check(ScheduledAdjustmentReservation item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentReservationRequiredTitleRule(string Message) : IScheduledAdjustmentReservationRule
{
    public List<string> Check(ScheduledAdjustmentReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled adjustment reservation.</summary>
public sealed record ScheduledAdjustmentReservationValidator(List<IScheduledAdjustmentReservationRule> Rules)
{
    public List<string> Validate(ScheduledAdjustmentReservation item)
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

    public void RequireValid(ScheduledAdjustmentReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAdjustmentReservation> items)
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

/// <summary>Functions of scheduled adjustment reservation rules.</summary>
public static class ScheduledAdjustmentReservationRules
{
    public static ScheduledAdjustmentReservationValidator DefaultScheduledAdjustmentReservationValidator()
    {
        return new ScheduledAdjustmentReservationValidator(
            new List<IScheduledAdjustmentReservationRule>
            {
                new ScheduledAdjustmentReservationCodeLengthRule(16),
                new ScheduledAdjustmentReservationCeilingRangeRule(0.4, 3438.8),
                new ScheduledAdjustmentReservationPriorityRangeRule(80, 120),
                new ScheduledAdjustmentReservationGraceDaysRangeRule(80, 3600),
                new ScheduledAdjustmentReservationRequiredTitleRule(
                    "scheduled adjustment reservation needs an entry"),
            });
    }
}
