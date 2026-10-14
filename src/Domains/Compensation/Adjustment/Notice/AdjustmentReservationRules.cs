namespace Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>A rule a adjustment reservation must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentReservationRule
{
    List<string> Check(AdjustmentReservation item);
}

public sealed record AdjustmentReservationNameLengthRule(int MaxLength) : IAdjustmentReservationRule
{
    public List<string> Check(AdjustmentReservation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentReservationRetriesRangeRule(int Min, int Max) : IAdjustmentReservationRule
{
    public List<string> Check(AdjustmentReservation item)
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

public sealed record AdjustmentReservationPriorityRangeRule(int Min, int Max) : IAdjustmentReservationRule
{
    public List<string> Check(AdjustmentReservation item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentReservationLeaveDaysRangeRule(int Min, int Max) : IAdjustmentReservationRule
{
    public List<string> Check(AdjustmentReservation item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentReservationSummaryLengthRule(int MaxLength) : IAdjustmentReservationRule
{
    public List<string> Check(AdjustmentReservation item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment reservation.</summary>
public sealed record AdjustmentReservationValidator(List<IAdjustmentReservationRule> Rules)
{
    public List<string> Validate(AdjustmentReservation item)
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

    public void RequireValid(AdjustmentReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentReservation> items)
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

/// <summary>Functions of adjustment reservation rules.</summary>
public static class AdjustmentReservationRules
{
    public static AdjustmentReservationValidator DefaultAdjustmentReservationValidator()
    {
        return new AdjustmentReservationValidator(
            new List<IAdjustmentReservationRule>
            {
                new AdjustmentReservationNameLengthRule(32),
                new AdjustmentReservationRetriesRangeRule(64, 86400),
                new AdjustmentReservationPriorityRangeRule(32, 80),
                new AdjustmentReservationLeaveDaysRangeRule(255, 1000),
                new AdjustmentReservationSummaryLengthRule(1000),
            });
    }
}
