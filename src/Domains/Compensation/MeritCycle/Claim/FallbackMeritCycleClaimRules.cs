namespace Acme.Workforce.Compensation.MeritCycle.Claim;

/// <summary>A rule a fallback merit cycle claim must pass. Check returns its problems, or none.</summary>
public interface IFallbackMeritCycleClaimRule
{
    List<string> Check(FallbackMeritCycleClaim item);
}

public sealed record FallbackMeritCycleClaimSourceLengthRule(int MaxLength) : IFallbackMeritCycleClaimRule
{
    public List<string> Check(FallbackMeritCycleClaim item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMeritCycleClaimRequiredLabelRule(string Message) : IFallbackMeritCycleClaimRule
{
    public List<string> Check(FallbackMeritCycleClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackMeritCycleClaimPriorityRangeRule(int Min, int Max) : IFallbackMeritCycleClaimRule
{
    public List<string> Check(FallbackMeritCycleClaim item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMeritCycleClaimLabelLengthRule(int MaxLength) : IFallbackMeritCycleClaimRule
{
    public List<string> Check(FallbackMeritCycleClaim item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback merit cycle claim.</summary>
public sealed record FallbackMeritCycleClaimValidator(List<IFallbackMeritCycleClaimRule> Rules)
{
    public List<string> Validate(FallbackMeritCycleClaim item)
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

    public void RequireValid(FallbackMeritCycleClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackMeritCycleClaim> items)
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

/// <summary>Functions of fallback merit cycle claim rules.</summary>
public static class FallbackMeritCycleClaimRules
{
    public static FallbackMeritCycleClaimValidator DefaultFallbackMeritCycleClaimValidator()
    {
        return new FallbackMeritCycleClaimValidator(
            new List<IFallbackMeritCycleClaimRule>
            {
                new FallbackMeritCycleClaimSourceLengthRule(80),
                new FallbackMeritCycleClaimRequiredLabelRule("fallback merit cycle claim needs an entry"),
                new FallbackMeritCycleClaimPriorityRangeRule(64, 1000),
                new FallbackMeritCycleClaimLabelLengthRule(86400),
            });
    }
}
