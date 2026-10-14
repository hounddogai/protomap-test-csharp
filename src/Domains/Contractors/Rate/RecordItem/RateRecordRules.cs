namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate record must pass. Check returns its problems, or none.</summary>
public interface IRateRecordRule
{
    List<string> Check(RateRecord item);
}

public sealed record RateRecordChannelLengthRule(int MaxLength) : IRateRecordRule
{
    public List<string> Check(RateRecord item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateRecordLimitRangeRule(int Min, int Max) : IRateRecordRule
{
    public List<string> Check(RateRecord item)
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

public sealed record RateRecordCodeLengthRule(int MaxLength) : IRateRecordRule
{
    public List<string> Check(RateRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateRecordCountryLengthRule(int MaxLength) : IRateRecordRule
{
    public List<string> Check(RateRecord item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateRecordAllowedChannelRule(List<string> Allowed) : IRateRecordRule
{
    public List<string> Check(RateRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is missing" };
        }
        return new List<string>();
    }
}

public sealed record RateRecordRequiredChannelRule(string Message) : IRateRecordRule
{
    public List<string> Check(RateRecord item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate record.</summary>
public sealed record RateRecordValidator(List<IRateRecordRule> Rules)
{
    public List<string> Validate(RateRecord item)
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

    public void RequireValid(RateRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateRecord> items)
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

/// <summary>Functions of rate record rules.</summary>
public static class RateRecordRules
{
    public static RateRecordValidator DefaultRateRecordValidator()
    {
        return new RateRecordValidator(
            new List<IRateRecordRule>
            {
                new RateRecordChannelLengthRule(3600),
                new RateRecordLimitRangeRule(255, 1000),
                new RateRecordCodeLengthRule(16),
                new RateRecordCountryLengthRule(80),
                new RateRecordAllowedChannelRule(new List<string> { "default" }),
                new RateRecordRequiredChannelRule("rate record needs an entry"),
            });
    }
}
