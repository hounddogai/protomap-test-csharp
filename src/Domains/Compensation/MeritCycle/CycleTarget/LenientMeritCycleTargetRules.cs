namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

/// <summary>A rule a lenient merit cycle target must pass. Check returns its problems, or none.</summary>
public interface ILenientMeritCycleTargetRule
{
    List<string> Check(LenientMeritCycleTarget item);
}

public sealed record LenientMeritCycleTargetCodeLengthRule(int MaxLength) : ILenientMeritCycleTargetRule
{
    public List<string> Check(LenientMeritCycleTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleTargetWeightRangeRule(double Min, double Max) : ILenientMeritCycleTargetRule
{
    public List<string> Check(LenientMeritCycleTarget item)
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

public sealed record LenientMeritCycleTargetCodePrefixRule(string Prefix) : ILenientMeritCycleTargetRule
{
    public List<string> Check(LenientMeritCycleTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleTargetRequiredCostCenterRule(string Message) : ILenientMeritCycleTargetRule
{
    public List<string> Check(LenientMeritCycleTarget item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient merit cycle target.</summary>
public sealed record LenientMeritCycleTargetValidator(List<ILenientMeritCycleTargetRule> Rules)
{
    public List<string> Validate(LenientMeritCycleTarget item)
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

    public void RequireValid(LenientMeritCycleTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientMeritCycleTarget> items)
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

/// <summary>Functions of lenient merit cycle target rules.</summary>
public static class LenientMeritCycleTargetRules
{
    public static LenientMeritCycleTargetValidator DefaultLenientMeritCycleTargetValidator()
    {
        return new LenientMeritCycleTargetValidator(
            new List<ILenientMeritCycleTargetRule>
            {
                new LenientMeritCycleTargetCodeLengthRule(255),
                new LenientMeritCycleTargetWeightRangeRule(3.5, 4584.6),
                new LenientMeritCycleTargetCodePrefixRule("REF"),
                new LenientMeritCycleTargetRequiredCostCenterRule("lenient merit cycle target needs an entry"),
            });
    }
}
