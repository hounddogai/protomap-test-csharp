namespace Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>A rule a merit cycle claim must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleClaimRule
{
    List<string> Check(MeritCycleClaim item);
}

public sealed record MeritCycleClaimRequiredCodeRule(string Message) : IMeritCycleClaimRule
{
    public List<string> Check(MeritCycleClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleClaimJobCodeLengthRule(int MaxLength) : IMeritCycleClaimRule
{
    public List<string> Check(MeritCycleClaim item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleClaimSourceLengthRule(int MaxLength) : IMeritCycleClaimRule
{
    public List<string> Check(MeritCycleClaim item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle claim.</summary>
public sealed record MeritCycleClaimValidator(List<IMeritCycleClaimRule> Rules)
{
    public List<string> Validate(MeritCycleClaim item)
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

    public void RequireValid(MeritCycleClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleClaim> items)
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

/// <summary>Functions of merit cycle claim rules.</summary>
public static class MeritCycleClaimRules
{
    public static MeritCycleClaimValidator DefaultMeritCycleClaimValidator()
    {
        return new MeritCycleClaimValidator(
            new List<IMeritCycleClaimRule>
            {
                new MeritCycleClaimRequiredCodeRule("merit cycle claim needs an entry"),
                new MeritCycleClaimJobCodeLengthRule(255),
                new MeritCycleClaimSourceLengthRule(32),
            });
    }
}
