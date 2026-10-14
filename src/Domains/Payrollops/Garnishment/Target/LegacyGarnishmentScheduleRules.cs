namespace Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>A rule a legacy garnishment schedule must pass. Check returns its problems, or none.</summary>
public interface ILegacyGarnishmentScheduleRule
{
    List<string> Check(LegacyGarnishmentSchedule item);
}

public sealed record LegacyGarnishmentScheduleLocaleLengthRule(int MaxLength) : ILegacyGarnishmentScheduleRule
{
    public List<string> Check(LegacyGarnishmentSchedule item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentScheduleCodeLengthRule(int MaxLength) : ILegacyGarnishmentScheduleRule
{
    public List<string> Check(LegacyGarnishmentSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentScheduleCodePrefixRule(string Prefix) : ILegacyGarnishmentScheduleRule
{
    public List<string> Check(LegacyGarnishmentSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentScheduleWeightRangeRule(double Min, double Max) : ILegacyGarnishmentScheduleRule
{
    public List<string> Check(LegacyGarnishmentSchedule item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyGarnishmentScheduleLeaveDaysRangeRule(int Min, int Max) : ILegacyGarnishmentScheduleRule
{
    public List<string> Check(LegacyGarnishmentSchedule item)
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

/// <summary>Runs every rule of a legacy garnishment schedule.</summary>
public sealed record LegacyGarnishmentScheduleValidator(List<ILegacyGarnishmentScheduleRule> Rules)
{
    public List<string> Validate(LegacyGarnishmentSchedule item)
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

    public void RequireValid(LegacyGarnishmentSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyGarnishmentSchedule> items)
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

/// <summary>Functions of legacy garnishment schedule rules.</summary>
public static class LegacyGarnishmentScheduleRules
{
    public static LegacyGarnishmentScheduleValidator DefaultLegacyGarnishmentScheduleValidator()
    {
        return new LegacyGarnishmentScheduleValidator(
            new List<ILegacyGarnishmentScheduleRule>
            {
                new LegacyGarnishmentScheduleLocaleLengthRule(16),
                new LegacyGarnishmentScheduleCodeLengthRule(1000),
                new LegacyGarnishmentScheduleCodePrefixRule("OPS"),
                new LegacyGarnishmentScheduleWeightRangeRule(8.7, 3755.5),
                new LegacyGarnishmentScheduleLeaveDaysRangeRule(255, 1000),
            });
    }
}
