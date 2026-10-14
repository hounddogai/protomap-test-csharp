namespace Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>A rule a merit cycle target must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleTargetRule
{
    List<string> Check(MeritCycleTarget item);
}

public sealed record MeritCycleTargetRequiredNameRule(string Message) : IMeritCycleTargetRule
{
    public List<string> Check(MeritCycleTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleTargetCodePrefixRule(string Prefix) : IMeritCycleTargetRule
{
    public List<string> Check(MeritCycleTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleTargetRequiredContactEmailRule(string Message) : IMeritCycleTargetRule
{
    public List<string> Check(MeritCycleTarget item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleTargetTimezoneLengthRule(int MaxLength) : IMeritCycleTargetRule
{
    public List<string> Check(MeritCycleTarget item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleTargetFloorValueRangeRule(double Min, double Max) : IMeritCycleTargetRule
{
    public List<string> Check(MeritCycleTarget item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleTargetRequiredTimezoneRule(string Message) : IMeritCycleTargetRule
{
    public List<string> Check(MeritCycleTarget item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle target.</summary>
public sealed record MeritCycleTargetValidator(List<IMeritCycleTargetRule> Rules)
{
    public List<string> Validate(MeritCycleTarget item)
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

    public void RequireValid(MeritCycleTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleTarget> items)
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

/// <summary>Functions of merit cycle target rules.</summary>
public static class MeritCycleTargetRules
{
    public static MeritCycleTargetValidator DefaultMeritCycleTargetValidator()
    {
        return new MeritCycleTargetValidator(
            new List<IMeritCycleTargetRule>
            {
                new MeritCycleTargetRequiredNameRule("merit cycle target needs this field"),
                new MeritCycleTargetCodePrefixRule("REF"),
                new MeritCycleTargetRequiredContactEmailRule("merit cycle target needs a value"),
                new MeritCycleTargetTimezoneLengthRule(86400),
                new MeritCycleTargetFloorValueRangeRule(4.4, 431.4),
                new MeritCycleTargetRequiredTimezoneRule("merit cycle target needs an entry"),
            });
    }
}
