namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility segment must pass. Check returns its problems, or none.</summary>
public interface IMobilitySegmentRule
{
    List<string> Check(MobilitySegment item);
}

public sealed record MobilitySegmentRetriesRangeRule(int Min, int Max) : IMobilitySegmentRule
{
    public List<string> Check(MobilitySegment item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilitySegmentRequiredCodeRule(string Message) : IMobilitySegmentRule
{
    public List<string> Check(MobilitySegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilitySegmentAllowedCodeRule(List<string> Allowed) : IMobilitySegmentRule
{
    public List<string> Check(MobilitySegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility segment.</summary>
public sealed record MobilitySegmentValidator(List<IMobilitySegmentRule> Rules)
{
    public List<string> Validate(MobilitySegment item)
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

    public void RequireValid(MobilitySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilitySegment> items)
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

/// <summary>Functions of mobility segment rules.</summary>
public static class MobilitySegmentRules
{
    public static MobilitySegmentValidator DefaultMobilitySegmentValidator()
    {
        return new MobilitySegmentValidator(
            new List<IMobilitySegmentRule>
            {
                new MobilitySegmentRetriesRangeRule(32, 120),
                new MobilitySegmentRequiredCodeRule("mobility segment needs a value"),
                new MobilitySegmentAllowedCodeRule(new List<string>()),
            });
    }
}
