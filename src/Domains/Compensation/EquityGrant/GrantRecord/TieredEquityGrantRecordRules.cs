namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered equity grant record must pass. Check returns its problems, or none.</summary>
public interface ITieredEquityGrantRecordRule
{
    List<string> Check(TieredEquityGrantRecord item);
}

public sealed record TieredEquityGrantRecordVersionRangeRule(int Min, int Max) : ITieredEquityGrantRecordRule
{
    public List<string> Check(TieredEquityGrantRecord item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantRecordAllowedCodeRule(List<string> Allowed) : ITieredEquityGrantRecordRule
{
    public List<string> Check(TieredEquityGrantRecord item)
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

public sealed record TieredEquityGrantRecordWindowDaysRangeRule(int Min, int Max) : ITieredEquityGrantRecordRule
{
    public List<string> Check(TieredEquityGrantRecord item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantRecordPinnedGuardRule(double Limit) : ITieredEquityGrantRecordRule
{
    public List<string> Check(TieredEquityGrantRecord item)
    {
        if (item.Pinned && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantRecordRequiredCodeRule(string Message) : ITieredEquityGrantRecordRule
{
    public List<string> Check(TieredEquityGrantRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered equity grant record.</summary>
public sealed record TieredEquityGrantRecordValidator(List<ITieredEquityGrantRecordRule> Rules)
{
    public List<string> Validate(TieredEquityGrantRecord item)
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

    public void RequireValid(TieredEquityGrantRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredEquityGrantRecord> items)
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

/// <summary>Functions of tiered equity grant record rules.</summary>
public static class TieredEquityGrantRecordRules
{
    public static TieredEquityGrantRecordValidator DefaultTieredEquityGrantRecordValidator()
    {
        return new TieredEquityGrantRecordValidator(
            new List<ITieredEquityGrantRecordRule>
            {
                new TieredEquityGrantRecordVersionRangeRule(64, 120),
                new TieredEquityGrantRecordAllowedCodeRule(new List<string> { "scheduled", "primary" }),
                new TieredEquityGrantRecordWindowDaysRangeRule(80, 255),
                new TieredEquityGrantRecordPinnedGuardRule(1.5),
                new TieredEquityGrantRecordRequiredCodeRule("tiered equity grant record needs a value"),
            });
    }
}
