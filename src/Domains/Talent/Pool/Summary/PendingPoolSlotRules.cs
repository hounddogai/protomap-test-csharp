namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending pool slot must pass. Check returns its problems, or none.</summary>
public interface IPendingPoolSlotRule
{
    List<string> Check(PendingPoolSlot item);
}

public sealed record PendingPoolSlotCodePrefixRule(string Prefix) : IPendingPoolSlotRule
{
    public List<string> Check(PendingPoolSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPoolSlotVersionRangeRule(int Min, int Max) : IPendingPoolSlotRule
{
    public List<string> Check(PendingPoolSlot item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingPoolSlotAllowedJobCodeRule(List<string> Allowed) : IPendingPoolSlotRule
{
    public List<string> Check(PendingPoolSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pool slot.</summary>
public sealed record PendingPoolSlotValidator(List<IPendingPoolSlotRule> Rules)
{
    public List<string> Validate(PendingPoolSlot item)
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

    public void RequireValid(PendingPoolSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPoolSlot> items)
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

/// <summary>Functions of pending pool slot rules.</summary>
public static class PendingPoolSlotRules
{
    public static PendingPoolSlotValidator DefaultPendingPoolSlotValidator()
    {
        return new PendingPoolSlotValidator(
            new List<IPendingPoolSlotRule>
            {
                new PendingPoolSlotCodePrefixRule("Q"),
                new PendingPoolSlotVersionRangeRule(32, 32),
                new PendingPoolSlotAllowedJobCodeRule(new List<string> { "weighted", "default" }),
            });
    }
}
