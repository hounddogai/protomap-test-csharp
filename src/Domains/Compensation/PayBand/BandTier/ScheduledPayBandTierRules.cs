namespace Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>A rule a scheduled pay band tier must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayBandTierRule
{
    List<string> Check(ScheduledPayBandTier item);
}

public sealed record ScheduledPayBandTierCodeLengthRule(int MaxLength) : IScheduledPayBandTierRule
{
    public List<string> Check(ScheduledPayBandTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandTierHoursRangeRule(double Min, double Max) : IScheduledPayBandTierRule
{
    public List<string> Check(ScheduledPayBandTier item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledPayBandTierLabelLengthRule(int MaxLength) : IScheduledPayBandTierRule
{
    public List<string> Check(ScheduledPayBandTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandTierRatioRangeRule(double Min, double Max) : IScheduledPayBandTierRule
{
    public List<string> Check(ScheduledPayBandTier item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledPayBandTierContactEmailLengthRule(int MaxLength) : IScheduledPayBandTierRule
{
    public List<string> Check(ScheduledPayBandTier item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pay band tier.</summary>
public sealed record ScheduledPayBandTierValidator(List<IScheduledPayBandTierRule> Rules)
{
    public List<string> Validate(ScheduledPayBandTier item)
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

    public void RequireValid(ScheduledPayBandTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayBandTier> items)
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

/// <summary>Functions of scheduled pay band tier rules.</summary>
public static class ScheduledPayBandTierRules
{
    public static ScheduledPayBandTierValidator DefaultScheduledPayBandTierValidator()
    {
        return new ScheduledPayBandTierValidator(
            new List<IScheduledPayBandTierRule>
            {
                new ScheduledPayBandTierCodeLengthRule(86400),
                new ScheduledPayBandTierHoursRangeRule(6.0, 2925.0),
                new ScheduledPayBandTierLabelLengthRule(1000),
                new ScheduledPayBandTierRatioRangeRule(6.6, 268.9),
                new ScheduledPayBandTierContactEmailLengthRule(120),
            });
    }
}
