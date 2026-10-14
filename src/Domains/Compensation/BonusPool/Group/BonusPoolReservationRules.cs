namespace Acme.Workforce.Compensation.BonusPool.Group;

/// <summary>A rule a bonus pool reservation must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolReservationRule
{
    List<string> Check(BonusPoolReservation item);
}

public sealed record BonusPoolReservationRequiredCodeRule(string Message) : IBonusPoolReservationRule
{
    public List<string> Check(BonusPoolReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolReservationDurationMinutesRangeRule(int Min, int Max) : IBonusPoolReservationRule
{
    public List<string> Check(BonusPoolReservation item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolReservationWindowDaysRangeRule(int Min, int Max) : IBonusPoolReservationRule
{
    public List<string> Check(BonusPoolReservation item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolReservationArchivedGuardRule(double Limit) : IBonusPoolReservationRule
{
    public List<string> Check(BonusPoolReservation item)
    {
        if (item.Archived && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolReservationBadgeLengthRule(int MaxLength) : IBonusPoolReservationRule
{
    public List<string> Check(BonusPoolReservation item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool reservation.</summary>
public sealed record BonusPoolReservationValidator(List<IBonusPoolReservationRule> Rules)
{
    public List<string> Validate(BonusPoolReservation item)
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

    public void RequireValid(BonusPoolReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolReservation> items)
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

/// <summary>Functions of bonus pool reservation rules.</summary>
public static class BonusPoolReservationRules
{
    public static BonusPoolReservationValidator DefaultBonusPoolReservationValidator()
    {
        return new BonusPoolReservationValidator(
            new List<IBonusPoolReservationRule>
            {
                new BonusPoolReservationRequiredCodeRule("bonus pool reservation needs this field"),
                new BonusPoolReservationDurationMinutesRangeRule(32, 86400),
                new BonusPoolReservationWindowDaysRangeRule(3600, 3600),
                new BonusPoolReservationArchivedGuardRule(9.1),
                new BonusPoolReservationBadgeLengthRule(64),
            });
    }
}
