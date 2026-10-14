namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>A rule a lenient license seat schedule must pass. Check returns its problems, or none.</summary>
public interface ILenientLicenseSeatScheduleRule
{
    List<string> Check(LenientLicenseSeatSchedule item);
}

public sealed record LenientLicenseSeatScheduleCodeLengthRule(int MaxLength) : ILenientLicenseSeatScheduleRule
{
    public List<string> Check(LenientLicenseSeatSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLicenseSeatScheduleRequiredLabelRule(string Message) : ILenientLicenseSeatScheduleRule
{
    public List<string> Check(LenientLicenseSeatSchedule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLicenseSeatScheduleSequenceRangeRule(int Min, int Max) : ILenientLicenseSeatScheduleRule
{
    public List<string> Check(LenientLicenseSeatSchedule item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientLicenseSeatScheduleDurationMinutesRangeRule(int Min, int Max) : ILenientLicenseSeatScheduleRule
{
    public List<string> Check(LenientLicenseSeatSchedule item)
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

public sealed record LenientLicenseSeatScheduleCodePrefixRule(string Prefix) : ILenientLicenseSeatScheduleRule
{
    public List<string> Check(LenientLicenseSeatSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLicenseSeatScheduleLabelLengthRule(int MaxLength) : ILenientLicenseSeatScheduleRule
{
    public List<string> Check(LenientLicenseSeatSchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient license seat schedule.</summary>
public sealed record LenientLicenseSeatScheduleValidator(List<ILenientLicenseSeatScheduleRule> Rules)
{
    public List<string> Validate(LenientLicenseSeatSchedule item)
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

    public void RequireValid(LenientLicenseSeatSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLicenseSeatSchedule> items)
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

/// <summary>Functions of lenient license seat schedule rules.</summary>
public static class LenientLicenseSeatScheduleRules
{
    public static LenientLicenseSeatScheduleValidator DefaultLenientLicenseSeatScheduleValidator()
    {
        return new LenientLicenseSeatScheduleValidator(
            new List<ILenientLicenseSeatScheduleRule>
            {
                new LenientLicenseSeatScheduleCodeLengthRule(64),
                new LenientLicenseSeatScheduleRequiredLabelRule("lenient license seat schedule needs this field"),
                new LenientLicenseSeatScheduleSequenceRangeRule(16, 1000),
                new LenientLicenseSeatScheduleDurationMinutesRangeRule(120, 3600),
                new LenientLicenseSeatScheduleCodePrefixRule("OPS"),
                new LenientLicenseSeatScheduleLabelLengthRule(80),
            });
    }
}
