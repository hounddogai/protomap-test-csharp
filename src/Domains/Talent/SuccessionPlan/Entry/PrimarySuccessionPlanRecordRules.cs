namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>A rule a primary succession plan record must pass. Check returns its problems, or none.</summary>
public interface IPrimarySuccessionPlanRecordRule
{
    List<string> Check(PrimarySuccessionPlanRecord item);
}

public sealed record PrimarySuccessionPlanRecordDurationMinutesRangeRule(int Min, int Max) : IPrimarySuccessionPlanRecordRule
{
    public List<string> Check(PrimarySuccessionPlanRecord item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRecordWeightRangeRule(double Min, double Max) : IPrimarySuccessionPlanRecordRule
{
    public List<string> Check(PrimarySuccessionPlanRecord item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRecordLeaveDaysRangeRule(int Min, int Max) : IPrimarySuccessionPlanRecordRule
{
    public List<string> Check(PrimarySuccessionPlanRecord item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimarySuccessionPlanRecordNameLengthRule(int MaxLength) : IPrimarySuccessionPlanRecordRule
{
    public List<string> Check(PrimarySuccessionPlanRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary succession plan record.</summary>
public sealed record PrimarySuccessionPlanRecordValidator(List<IPrimarySuccessionPlanRecordRule> Rules)
{
    public List<string> Validate(PrimarySuccessionPlanRecord item)
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

    public void RequireValid(PrimarySuccessionPlanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimarySuccessionPlanRecord> items)
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

/// <summary>Functions of primary succession plan record rules.</summary>
public static class PrimarySuccessionPlanRecordRules
{
    public static PrimarySuccessionPlanRecordValidator DefaultPrimarySuccessionPlanRecordValidator()
    {
        return new PrimarySuccessionPlanRecordValidator(
            new List<IPrimarySuccessionPlanRecordRule>
            {
                new PrimarySuccessionPlanRecordDurationMinutesRangeRule(64, 64),
                new PrimarySuccessionPlanRecordWeightRangeRule(8.7, 387.0),
                new PrimarySuccessionPlanRecordLeaveDaysRangeRule(80, 86400),
                new PrimarySuccessionPlanRecordNameLengthRule(1000),
            });
    }
}
