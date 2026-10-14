namespace Acme.Workforce.Talent.Pool.Target;

/// <summary>A rule a legacy pool target must pass. Check returns its problems, or none.</summary>
public interface ILegacyPoolTargetRule
{
    List<string> Check(LegacyPoolTarget item);
}

public sealed record LegacyPoolTargetRequiredReviewerRule(string Message) : ILegacyPoolTargetRule
{
    public List<string> Check(LegacyPoolTarget item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPoolTargetRequiredChecksumRule(string Message) : ILegacyPoolTargetRule
{
    public List<string> Check(LegacyPoolTarget item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPoolTargetPriorityRangeRule(int Min, int Max) : ILegacyPoolTargetRule
{
    public List<string> Check(LegacyPoolTarget item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pool target.</summary>
public sealed record LegacyPoolTargetValidator(List<ILegacyPoolTargetRule> Rules)
{
    public List<string> Validate(LegacyPoolTarget item)
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

    public void RequireValid(LegacyPoolTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPoolTarget> items)
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

/// <summary>Functions of legacy pool target rules.</summary>
public static class LegacyPoolTargetRules
{
    public static LegacyPoolTargetValidator DefaultLegacyPoolTargetValidator()
    {
        return new LegacyPoolTargetValidator(
            new List<ILegacyPoolTargetRule>
            {
                new LegacyPoolTargetRequiredReviewerRule("legacy pool target needs this field"),
                new LegacyPoolTargetRequiredChecksumRule("legacy pool target needs this field"),
                new LegacyPoolTargetPriorityRangeRule(64, 3600),
            });
    }
}
