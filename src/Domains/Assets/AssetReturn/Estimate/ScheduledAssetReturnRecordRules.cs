namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled asset return record must pass. Check returns its problems, or none.</summary>
public interface IScheduledAssetReturnRecordRule
{
    List<string> Check(ScheduledAssetReturnRecord item);
}

public sealed record ScheduledAssetReturnRecordAllowedLocationRule(List<string> Allowed) : IScheduledAssetReturnRecordRule
{
    public List<string> Check(ScheduledAssetReturnRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnRecordRatioRangeRule(double Min, double Max) : IScheduledAssetReturnRecordRule
{
    public List<string> Check(ScheduledAssetReturnRecord item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledAssetReturnRecordRequiredJobCodeRule(string Message) : IScheduledAssetReturnRecordRule
{
    public List<string> Check(ScheduledAssetReturnRecord item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnRecordCodePrefixRule(string Prefix) : IScheduledAssetReturnRecordRule
{
    public List<string> Check(ScheduledAssetReturnRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled asset return record.</summary>
public sealed record ScheduledAssetReturnRecordValidator(List<IScheduledAssetReturnRecordRule> Rules)
{
    public List<string> Validate(ScheduledAssetReturnRecord item)
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

    public void RequireValid(ScheduledAssetReturnRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAssetReturnRecord> items)
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

/// <summary>Functions of scheduled asset return record rules.</summary>
public static class ScheduledAssetReturnRecordRules
{
    public static ScheduledAssetReturnRecordValidator DefaultScheduledAssetReturnRecordValidator()
    {
        return new ScheduledAssetReturnRecordValidator(
            new List<IScheduledAssetReturnRecordRule>
            {
                new ScheduledAssetReturnRecordAllowedLocationRule(new List<string> { "legacy", "default" }),
                new ScheduledAssetReturnRecordRatioRangeRule(6.3, 4494.1),
                new ScheduledAssetReturnRecordRequiredJobCodeRule("scheduled asset return record needs this field"),
                new ScheduledAssetReturnRecordCodePrefixRule("REF"),
            });
    }
}
