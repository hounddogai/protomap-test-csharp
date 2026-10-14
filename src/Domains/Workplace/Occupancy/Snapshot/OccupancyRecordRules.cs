namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy record must pass. Check returns its problems, or none.</summary>
public interface IOccupancyRecordRule
{
    List<string> Check(OccupancyRecord item);
}

public sealed record OccupancyRecordRequiredTitleRule(string Message) : IOccupancyRecordRule
{
    public List<string> Check(OccupancyRecord item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRecordTenureMonthsRangeRule(int Min, int Max) : IOccupancyRecordRule
{
    public List<string> Check(OccupancyRecord item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRecordAllowedCodeRule(List<string> Allowed) : IOccupancyRecordRule
{
    public List<string> Check(OccupancyRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRecordCodePrefixRule(string Prefix) : IOccupancyRecordRule
{
    public List<string> Check(OccupancyRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRecordRateRangeRule(double Min, double Max) : IOccupancyRecordRule
{
    public List<string> Check(OccupancyRecord item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy record.</summary>
public sealed record OccupancyRecordValidator(List<IOccupancyRecordRule> Rules)
{
    public List<string> Validate(OccupancyRecord item)
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

    public void RequireValid(OccupancyRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyRecord> items)
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

/// <summary>Functions of occupancy record rules.</summary>
public static class OccupancyRecordRules
{
    public static OccupancyRecordValidator DefaultOccupancyRecordValidator()
    {
        return new OccupancyRecordValidator(
            new List<IOccupancyRecordRule>
            {
                new OccupancyRecordRequiredTitleRule("occupancy record needs this field"),
                new OccupancyRecordTenureMonthsRangeRule(32, 80),
                new OccupancyRecordAllowedCodeRule(new List<string>()),
                new OccupancyRecordCodePrefixRule("OPS"),
                new OccupancyRecordRateRangeRule(2.8, 2780.5),
            });
    }
}
