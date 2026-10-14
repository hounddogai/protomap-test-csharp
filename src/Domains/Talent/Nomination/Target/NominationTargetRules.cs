namespace Acme.Workforce.Talent.Nomination.Target;

/// <summary>A rule a nomination target must pass. Check returns its problems, or none.</summary>
public interface INominationTargetRule
{
    List<string> Check(NominationTarget item);
}

public sealed record NominationTargetThresholdRangeRule(double Min, double Max) : INominationTargetRule
{
    public List<string> Check(NominationTarget item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record NominationTargetRequiredCodeRule(string Message) : INominationTargetRule
{
    public List<string> Check(NominationTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationTargetRequiredTitleRule(string Message) : INominationTargetRule
{
    public List<string> Check(NominationTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationTargetNameLengthRule(int MaxLength) : INominationTargetRule
{
    public List<string> Check(NominationTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationTargetRequiredChecksumRule(string Message) : INominationTargetRule
{
    public List<string> Check(NominationTarget item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationTargetTitleLengthRule(int MaxLength) : INominationTargetRule
{
    public List<string> Check(NominationTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination target.</summary>
public sealed record NominationTargetValidator(List<INominationTargetRule> Rules)
{
    public List<string> Validate(NominationTarget item)
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

    public void RequireValid(NominationTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationTarget> items)
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

/// <summary>Functions of nomination target rules.</summary>
public static class NominationTargetRules
{
    public static NominationTargetValidator DefaultNominationTargetValidator()
    {
        return new NominationTargetValidator(
            new List<INominationTargetRule>
            {
                new NominationTargetThresholdRangeRule(7.4, 2709.8),
                new NominationTargetRequiredCodeRule("nomination target needs this field"),
                new NominationTargetRequiredTitleRule("nomination target needs this field"),
                new NominationTargetNameLengthRule(32),
                new NominationTargetRequiredChecksumRule("nomination target needs this field"),
                new NominationTargetTitleLengthRule(80),
            });
    }
}
