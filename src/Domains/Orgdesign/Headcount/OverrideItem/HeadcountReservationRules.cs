namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>A rule a headcount reservation must pass. Check returns its problems, or none.</summary>
public interface IHeadcountReservationRule
{
    List<string> Check(HeadcountReservation item);
}

public sealed record HeadcountReservationRequiredTitleRule(string Message) : IHeadcountReservationRule
{
    public List<string> Check(HeadcountReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountReservationAttemptsRangeRule(int Min, int Max) : IHeadcountReservationRule
{
    public List<string> Check(HeadcountReservation item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountReservationCodePrefixRule(string Prefix) : IHeadcountReservationRule
{
    public List<string> Check(HeadcountReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountReservationLimitRangeRule(int Min, int Max) : IHeadcountReservationRule
{
    public List<string> Check(HeadcountReservation item)
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

public sealed record HeadcountReservationPinnedGuardRule(int Limit) : IHeadcountReservationRule
{
    public List<string> Check(HeadcountReservation item)
    {
        if (item.Pinned && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountReservationCodeLengthRule(int MaxLength) : IHeadcountReservationRule
{
    public List<string> Check(HeadcountReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount reservation.</summary>
public sealed record HeadcountReservationValidator(List<IHeadcountReservationRule> Rules)
{
    public List<string> Validate(HeadcountReservation item)
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

    public void RequireValid(HeadcountReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountReservation> items)
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

/// <summary>Functions of headcount reservation rules.</summary>
public static class HeadcountReservationRules
{
    public static HeadcountReservationValidator DefaultHeadcountReservationValidator()
    {
        return new HeadcountReservationValidator(
            new List<IHeadcountReservationRule>
            {
                new HeadcountReservationRequiredTitleRule("headcount reservation needs a value"),
                new HeadcountReservationAttemptsRangeRule(80, 86400),
                new HeadcountReservationCodePrefixRule("REF"),
                new HeadcountReservationLimitRangeRule(255, 1000),
                new HeadcountReservationPinnedGuardRule(64),
                new HeadcountReservationCodeLengthRule(32),
            });
    }
}
