namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>A rule a license seat reservation must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatReservationRule
{
    List<string> Check(LicenseSeatReservation item);
}

public sealed record LicenseSeatReservationTitleLengthRule(int MaxLength) : ILicenseSeatReservationRule
{
    public List<string> Check(LicenseSeatReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatReservationTagsLimitRule(int MaxCount, List<string> Banned) : ILicenseSeatReservationRule
{
    public List<string> Check(LicenseSeatReservation item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record LicenseSeatReservationRequiredCodeRule(string Message) : ILicenseSeatReservationRule
{
    public List<string> Check(LicenseSeatReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatReservationWindowDaysRangeRule(int Min, int Max) : ILicenseSeatReservationRule
{
    public List<string> Check(LicenseSeatReservation item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatReservationGraceDaysRangeRule(int Min, int Max) : ILicenseSeatReservationRule
{
    public List<string> Check(LicenseSeatReservation item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a license seat reservation.</summary>
public sealed record LicenseSeatReservationValidator(List<ILicenseSeatReservationRule> Rules)
{
    public List<string> Validate(LicenseSeatReservation item)
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

    public void RequireValid(LicenseSeatReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatReservation> items)
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

/// <summary>Functions of license seat reservation rules.</summary>
public static class LicenseSeatReservationRules
{
    public static LicenseSeatReservationValidator DefaultLicenseSeatReservationValidator()
    {
        return new LicenseSeatReservationValidator(
            new List<ILicenseSeatReservationRule>
            {
                new LicenseSeatReservationTitleLengthRule(32),
                new LicenseSeatReservationTagsLimitRule(1000, new List<string>()),
                new LicenseSeatReservationRequiredCodeRule("license seat reservation needs this field"),
                new LicenseSeatReservationWindowDaysRangeRule(80, 3600),
                new LicenseSeatReservationGraceDaysRangeRule(1000, 86400),
            });
    }
}
