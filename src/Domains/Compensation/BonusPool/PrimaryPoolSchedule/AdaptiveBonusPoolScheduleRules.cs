namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>A rule a adaptive bonus pool schedule must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBonusPoolScheduleRule
{
    List<string> Check(AdaptiveBonusPoolSchedule item);
}

public sealed record AdaptiveBonusPoolScheduleNotesLengthRule(int MaxLength) : IAdaptiveBonusPoolScheduleRule
{
    public List<string> Check(AdaptiveBonusPoolSchedule item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolSchedulePriorityRangeRule(int Min, int Max) : IAdaptiveBonusPoolScheduleRule
{
    public List<string> Check(AdaptiveBonusPoolSchedule item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolScheduleNameLengthRule(int MaxLength) : IAdaptiveBonusPoolScheduleRule
{
    public List<string> Check(AdaptiveBonusPoolSchedule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolScheduleRequiredCountryRule(string Message) : IAdaptiveBonusPoolScheduleRule
{
    public List<string> Check(AdaptiveBonusPoolSchedule item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolScheduleCodePrefixRule(string Prefix) : IAdaptiveBonusPoolScheduleRule
{
    public List<string> Check(AdaptiveBonusPoolSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive bonus pool schedule.</summary>
public sealed record AdaptiveBonusPoolScheduleValidator(List<IAdaptiveBonusPoolScheduleRule> Rules)
{
    public List<string> Validate(AdaptiveBonusPoolSchedule item)
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

    public void RequireValid(AdaptiveBonusPoolSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBonusPoolSchedule> items)
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

/// <summary>Functions of adaptive bonus pool schedule rules.</summary>
public static class AdaptiveBonusPoolScheduleRules
{
    public static AdaptiveBonusPoolScheduleValidator DefaultAdaptiveBonusPoolScheduleValidator()
    {
        return new AdaptiveBonusPoolScheduleValidator(
            new List<IAdaptiveBonusPoolScheduleRule>
            {
                new AdaptiveBonusPoolScheduleNotesLengthRule(16),
                new AdaptiveBonusPoolSchedulePriorityRangeRule(16, 3600),
                new AdaptiveBonusPoolScheduleNameLengthRule(255),
                new AdaptiveBonusPoolScheduleRequiredCountryRule("adaptive bonus pool schedule needs an entry"),
                new AdaptiveBonusPoolScheduleCodePrefixRule("OPS"),
            });
    }
}
