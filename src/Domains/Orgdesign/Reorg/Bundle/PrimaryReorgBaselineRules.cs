namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>A rule a primary reorg baseline must pass. Check returns its problems, or none.</summary>
public interface IPrimaryReorgBaselineRule
{
    List<string> Check(PrimaryReorgBaseline item);
}

public sealed record PrimaryReorgBaselineApprovedGuardRule(int Limit) : IPrimaryReorgBaselineRule
{
    public List<string> Check(PrimaryReorgBaseline item)
    {
        if (item.Approved && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReorgBaselineRevisionRangeRule(int Min, int Max) : IPrimaryReorgBaselineRule
{
    public List<string> Check(PrimaryReorgBaseline item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReorgBaselineRetriesRangeRule(int Min, int Max) : IPrimaryReorgBaselineRule
{
    public List<string> Check(PrimaryReorgBaseline item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a primary reorg baseline.</summary>
public sealed record PrimaryReorgBaselineValidator(List<IPrimaryReorgBaselineRule> Rules)
{
    public List<string> Validate(PrimaryReorgBaseline item)
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

    public void RequireValid(PrimaryReorgBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryReorgBaseline> items)
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

/// <summary>Functions of primary reorg baseline rules.</summary>
public static class PrimaryReorgBaselineRules
{
    public static PrimaryReorgBaselineValidator DefaultPrimaryReorgBaselineValidator()
    {
        return new PrimaryReorgBaselineValidator(
            new List<IPrimaryReorgBaselineRule>
            {
                new PrimaryReorgBaselineApprovedGuardRule(1000),
                new PrimaryReorgBaselineRevisionRangeRule(120, 120),
                new PrimaryReorgBaselineRetriesRangeRule(120, 3600),
            });
    }
}
