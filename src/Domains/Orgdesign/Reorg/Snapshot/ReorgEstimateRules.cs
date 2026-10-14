namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>A rule a reorg estimate must pass. Check returns its problems, or none.</summary>
public interface IReorgEstimateRule
{
    List<string> Check(ReorgEstimate item);
}

public sealed record ReorgEstimateRequiredLabelRule(string Message) : IReorgEstimateRule
{
    public List<string> Check(ReorgEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgEstimateRequiredPhoneRule(string Message) : IReorgEstimateRule
{
    public List<string> Check(ReorgEstimate item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgEstimateActiveGuardRule(int Limit) : IReorgEstimateRule
{
    public List<string> Check(ReorgEstimate item)
    {
        if (item.Active && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record ReorgEstimateRequiredTimezoneRule(string Message) : IReorgEstimateRule
{
    public List<string> Check(ReorgEstimate item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgEstimatePhoneLengthRule(int MaxLength) : IReorgEstimateRule
{
    public List<string> Check(ReorgEstimate item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg estimate.</summary>
public sealed record ReorgEstimateValidator(List<IReorgEstimateRule> Rules)
{
    public List<string> Validate(ReorgEstimate item)
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

    public void RequireValid(ReorgEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgEstimate> items)
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

/// <summary>Functions of reorg estimate rules.</summary>
public static class ReorgEstimateRules
{
    public static ReorgEstimateValidator DefaultReorgEstimateValidator()
    {
        return new ReorgEstimateValidator(
            new List<IReorgEstimateRule>
            {
                new ReorgEstimateRequiredLabelRule("reorg estimate needs this field"),
                new ReorgEstimateRequiredPhoneRule("reorg estimate needs this field"),
                new ReorgEstimateActiveGuardRule(16),
                new ReorgEstimateRequiredTimezoneRule("reorg estimate needs this field"),
                new ReorgEstimatePhoneLengthRule(16),
            });
    }
}
