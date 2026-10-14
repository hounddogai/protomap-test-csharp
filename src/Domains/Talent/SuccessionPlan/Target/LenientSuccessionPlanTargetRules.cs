namespace Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>A rule a lenient succession plan target must pass. Check returns its problems, or none.</summary>
public interface ILenientSuccessionPlanTargetRule
{
    List<string> Check(LenientSuccessionPlanTarget item);
}

public sealed record LenientSuccessionPlanTargetCodeLengthRule(int MaxLength) : ILenientSuccessionPlanTargetRule
{
    public List<string> Check(LenientSuccessionPlanTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanTargetCodePrefixRule(string Prefix) : ILenientSuccessionPlanTargetRule
{
    public List<string> Check(LenientSuccessionPlanTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanTargetRequiredCodeRule(string Message) : ILenientSuccessionPlanTargetRule
{
    public List<string> Check(LenientSuccessionPlanTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient succession plan target.</summary>
public sealed record LenientSuccessionPlanTargetValidator(List<ILenientSuccessionPlanTargetRule> Rules)
{
    public List<string> Validate(LenientSuccessionPlanTarget item)
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

    public void RequireValid(LenientSuccessionPlanTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSuccessionPlanTarget> items)
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

/// <summary>Functions of lenient succession plan target rules.</summary>
public static class LenientSuccessionPlanTargetRules
{
    public static LenientSuccessionPlanTargetValidator DefaultLenientSuccessionPlanTargetValidator()
    {
        return new LenientSuccessionPlanTargetValidator(
            new List<ILenientSuccessionPlanTargetRule>
            {
                new LenientSuccessionPlanTargetCodeLengthRule(86400),
                new LenientSuccessionPlanTargetCodePrefixRule("OPS"),
                new LenientSuccessionPlanTargetRequiredCodeRule("lenient succession plan target needs this field"),
            });
    }
}
