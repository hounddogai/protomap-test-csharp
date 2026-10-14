namespace Acme.Workforce.Engagement.TownHall.Window;

/// <summary>A rule a town hall reservation must pass. Check returns its problems, or none.</summary>
public interface ITownHallReservationRule
{
    List<string> Check(TownHallReservation item);
}

public sealed record TownHallReservationRequiredLabelRule(string Message) : ITownHallReservationRule
{
    public List<string> Check(TownHallReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallReservationRetriesRangeRule(int Min, int Max) : ITownHallReservationRule
{
    public List<string> Check(TownHallReservation item)
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

public sealed record TownHallReservationCreatedAtRangeRule(int Min, int Max) : ITownHallReservationRule
{
    public List<string> Check(TownHallReservation item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallReservationRequiredLocationRule(string Message) : ITownHallReservationRule
{
    public List<string> Check(TownHallReservation item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallReservationContactEmailLengthRule(int MaxLength) : ITownHallReservationRule
{
    public List<string> Check(TownHallReservation item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallReservationBatchSizeRangeRule(int Min, int Max) : ITownHallReservationRule
{
    public List<string> Check(TownHallReservation item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a town hall reservation.</summary>
public sealed record TownHallReservationValidator(List<ITownHallReservationRule> Rules)
{
    public List<string> Validate(TownHallReservation item)
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

    public void RequireValid(TownHallReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallReservation> items)
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

/// <summary>Functions of town hall reservation rules.</summary>
public static class TownHallReservationRules
{
    public static TownHallReservationValidator DefaultTownHallReservationValidator()
    {
        return new TownHallReservationValidator(
            new List<ITownHallReservationRule>
            {
                new TownHallReservationRequiredLabelRule("town hall reservation needs this field"),
                new TownHallReservationRetriesRangeRule(1000, 86400),
                new TownHallReservationCreatedAtRangeRule(16, 32),
                new TownHallReservationRequiredLocationRule("town hall reservation needs an entry"),
                new TownHallReservationContactEmailLengthRule(80),
                new TownHallReservationBatchSizeRangeRule(32, 80),
            });
    }
}
