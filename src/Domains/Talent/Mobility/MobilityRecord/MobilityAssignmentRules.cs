namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility assignment must pass. Check returns its problems, or none.</summary>
public interface IMobilityAssignmentRule
{
    List<string> Check(MobilityAssignment item);
}

public sealed record MobilityAssignmentRetriesRangeRule(int Min, int Max) : IMobilityAssignmentRule
{
    public List<string> Check(MobilityAssignment item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityAssignmentCodePrefixRule(string Prefix) : IMobilityAssignmentRule
{
    public List<string> Check(MobilityAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityAssignmentAllowedLabelRule(List<string> Allowed) : IMobilityAssignmentRule
{
    public List<string> Check(MobilityAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

public sealed record MobilityAssignmentRateRangeRule(double Min, double Max) : IMobilityAssignmentRule
{
    public List<string> Check(MobilityAssignment item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility assignment.</summary>
public sealed record MobilityAssignmentValidator(List<IMobilityAssignmentRule> Rules)
{
    public List<string> Validate(MobilityAssignment item)
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

    public void RequireValid(MobilityAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityAssignment> items)
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

/// <summary>Functions of mobility assignment rules.</summary>
public static class MobilityAssignmentRules
{
    public static MobilityAssignmentValidator DefaultMobilityAssignmentValidator()
    {
        return new MobilityAssignmentValidator(
            new List<IMobilityAssignmentRule>
            {
                new MobilityAssignmentRetriesRangeRule(16, 3600),
                new MobilityAssignmentCodePrefixRule("ACM"),
                new MobilityAssignmentAllowedLabelRule(new List<string> { "legacy", "weighted" }),
                new MobilityAssignmentRateRangeRule(2.2, 2127.0),
            });
    }
}
