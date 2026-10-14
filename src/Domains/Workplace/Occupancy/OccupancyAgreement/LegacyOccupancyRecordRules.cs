namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>A rule a legacy occupancy record must pass. Check returns its problems, or none.</summary>
public interface ILegacyOccupancyRecordRule
{
    List<string> Check(LegacyOccupancyRecord item);
}

public sealed record LegacyOccupancyRecordRequiredCategoryRule(string Message) : ILegacyOccupancyRecordRule
{
    public List<string> Check(LegacyOccupancyRecord item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyOccupancyRecordExpiresAtRangeRule(int Min, int Max) : ILegacyOccupancyRecordRule
{
    public List<string> Check(LegacyOccupancyRecord item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyOccupancyRecordCodeLengthRule(int MaxLength) : ILegacyOccupancyRecordRule
{
    public List<string> Check(LegacyOccupancyRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOccupancyRecordWeightRangeRule(double Min, double Max) : ILegacyOccupancyRecordRule
{
    public List<string> Check(LegacyOccupancyRecord item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOccupancyRecordSummaryLengthRule(int MaxLength) : ILegacyOccupancyRecordRule
{
    public List<string> Check(LegacyOccupancyRecord item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy occupancy record.</summary>
public sealed record LegacyOccupancyRecordValidator(List<ILegacyOccupancyRecordRule> Rules)
{
    public List<string> Validate(LegacyOccupancyRecord item)
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

    public void RequireValid(LegacyOccupancyRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyOccupancyRecord> items)
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

/// <summary>Functions of legacy occupancy record rules.</summary>
public static class LegacyOccupancyRecordRules
{
    public static LegacyOccupancyRecordValidator DefaultLegacyOccupancyRecordValidator()
    {
        return new LegacyOccupancyRecordValidator(
            new List<ILegacyOccupancyRecordRule>
            {
                new LegacyOccupancyRecordRequiredCategoryRule("legacy occupancy record needs an entry"),
                new LegacyOccupancyRecordExpiresAtRangeRule(80, 1000),
                new LegacyOccupancyRecordCodeLengthRule(64),
                new LegacyOccupancyRecordWeightRangeRule(1.2, 4861.2),
                new LegacyOccupancyRecordSummaryLengthRule(80),
            });
    }
}
