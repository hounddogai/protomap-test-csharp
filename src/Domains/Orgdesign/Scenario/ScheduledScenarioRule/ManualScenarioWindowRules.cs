namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>A rule a manual scenario window must pass. Check returns its problems, or none.</summary>
public interface IManualScenarioWindowRule
{
    List<string> Check(ManualScenarioWindow item);
}

public sealed record ManualScenarioWindowJobCodeLengthRule(int MaxLength) : IManualScenarioWindowRule
{
    public List<string> Check(ManualScenarioWindow item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioWindowRequiredCodeRule(string Message) : IManualScenarioWindowRule
{
    public List<string> Check(ManualScenarioWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioWindowEndsAtRangeRule(int Min, int Max) : IManualScenarioWindowRule
{
    public List<string> Check(ManualScenarioWindow item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioWindowRequiredJobCodeRule(string Message) : IManualScenarioWindowRule
{
    public List<string> Check(ManualScenarioWindow item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioWindowRequiredTimezoneRule(string Message) : IManualScenarioWindowRule
{
    public List<string> Check(ManualScenarioWindow item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioWindowWindowDaysRangeRule(int Min, int Max) : IManualScenarioWindowRule
{
    public List<string> Check(ManualScenarioWindow item)
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

/// <summary>Runs every rule of a manual scenario window.</summary>
public sealed record ManualScenarioWindowValidator(List<IManualScenarioWindowRule> Rules)
{
    public List<string> Validate(ManualScenarioWindow item)
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

    public void RequireValid(ManualScenarioWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualScenarioWindow> items)
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

/// <summary>Functions of manual scenario window rules.</summary>
public static class ManualScenarioWindowRules
{
    public static ManualScenarioWindowValidator DefaultManualScenarioWindowValidator()
    {
        return new ManualScenarioWindowValidator(
            new List<IManualScenarioWindowRule>
            {
                new ManualScenarioWindowJobCodeLengthRule(32),
                new ManualScenarioWindowRequiredCodeRule("manual scenario window needs this field"),
                new ManualScenarioWindowEndsAtRangeRule(64, 255),
                new ManualScenarioWindowRequiredJobCodeRule("manual scenario window needs a value"),
                new ManualScenarioWindowRequiredTimezoneRule("manual scenario window needs an entry"),
                new ManualScenarioWindowWindowDaysRangeRule(64, 86400),
            });
    }
}
