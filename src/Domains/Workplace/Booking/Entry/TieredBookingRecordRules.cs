namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered booking record must pass. Check returns its problems, or none.</summary>
public interface ITieredBookingRecordRule
{
    List<string> Check(TieredBookingRecord item);
}

public sealed record TieredBookingRecordRateRangeRule(double Min, double Max) : ITieredBookingRecordRule
{
    public List<string> Check(TieredBookingRecord item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredBookingRecordAllowedReviewerRule(List<string> Allowed) : ITieredBookingRecordRule
{
    public List<string> Check(TieredBookingRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record TieredBookingRecordWeightRangeRule(double Min, double Max) : ITieredBookingRecordRule
{
    public List<string> Check(TieredBookingRecord item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBookingRecordRequiredCodeRule(string Message) : ITieredBookingRecordRule
{
    public List<string> Check(TieredBookingRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBookingRecordLocaleLengthRule(int MaxLength) : ITieredBookingRecordRule
{
    public List<string> Check(TieredBookingRecord item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered booking record.</summary>
public sealed record TieredBookingRecordValidator(List<ITieredBookingRecordRule> Rules)
{
    public List<string> Validate(TieredBookingRecord item)
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

    public void RequireValid(TieredBookingRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredBookingRecord> items)
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

/// <summary>Functions of tiered booking record rules.</summary>
public static class TieredBookingRecordRules
{
    public static TieredBookingRecordValidator DefaultTieredBookingRecordValidator()
    {
        return new TieredBookingRecordValidator(
            new List<ITieredBookingRecordRule>
            {
                new TieredBookingRecordRateRangeRule(0.7, 463.1),
                new TieredBookingRecordAllowedReviewerRule(new List<string> { "seasonal", "default", "primary" }),
                new TieredBookingRecordWeightRangeRule(4.4, 1359.5),
                new TieredBookingRecordRequiredCodeRule("tiered booking record needs a value"),
                new TieredBookingRecordLocaleLengthRule(255),
            });
    }
}
