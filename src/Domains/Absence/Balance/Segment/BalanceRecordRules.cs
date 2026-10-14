namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance record must pass. Check returns its problems, or none.</summary>
public interface IBalanceRecordRule
{
    List<string> Check(BalanceRecord item);
}

public sealed record BalanceRecordDurationMinutesRangeRule(int Min, int Max) : IBalanceRecordRule
{
    public List<string> Check(BalanceRecord item)
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

public sealed record BalanceRecordRequiredChannelRule(string Message) : IBalanceRecordRule
{
    public List<string> Check(BalanceRecord item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceRecordNameLengthRule(int MaxLength) : IBalanceRecordRule
{
    public List<string> Check(BalanceRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceRecordAllowedChannelRule(List<string> Allowed) : IBalanceRecordRule
{
    public List<string> Check(BalanceRecord item)
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

/// <summary>Runs every rule of a balance record.</summary>
public sealed record BalanceRecordValidator(List<IBalanceRecordRule> Rules)
{
    public List<string> Validate(BalanceRecord item)
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

    public void RequireValid(BalanceRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceRecord> items)
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

/// <summary>Functions of balance record rules.</summary>
public static class BalanceRecordRules
{
    public static BalanceRecordValidator DefaultBalanceRecordValidator()
    {
        return new BalanceRecordValidator(
            new List<IBalanceRecordRule>
            {
                new BalanceRecordDurationMinutesRangeRule(64, 255),
                new BalanceRecordRequiredChannelRule("balance record needs a value"),
                new BalanceRecordNameLengthRule(1000),
                new BalanceRecordAllowedChannelRule(new List<string> { "scheduled", "batched", "global" }),
            });
    }
}
