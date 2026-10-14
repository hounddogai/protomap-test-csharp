namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>A rule a legacy leave request record must pass. Check returns its problems, or none.</summary>
public interface ILegacyLeaveRequestRecordRule
{
    List<string> Check(LegacyLeaveRequestRecord item);
}

public sealed record LegacyLeaveRequestRecordRequiredNameRule(string Message) : ILegacyLeaveRequestRecordRule
{
    public List<string> Check(LegacyLeaveRequestRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestRecordExpiresAtRangeRule(int Min, int Max) : ILegacyLeaveRequestRecordRule
{
    public List<string> Check(LegacyLeaveRequestRecord item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestRecordRatioRangeRule(double Min, double Max) : ILegacyLeaveRequestRecordRule
{
    public List<string> Check(LegacyLeaveRequestRecord item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestRecordCodeLengthRule(int MaxLength) : ILegacyLeaveRequestRecordRule
{
    public List<string> Check(LegacyLeaveRequestRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy leave request record.</summary>
public sealed record LegacyLeaveRequestRecordValidator(List<ILegacyLeaveRequestRecordRule> Rules)
{
    public List<string> Validate(LegacyLeaveRequestRecord item)
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

    public void RequireValid(LegacyLeaveRequestRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyLeaveRequestRecord> items)
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

/// <summary>Functions of legacy leave request record rules.</summary>
public static class LegacyLeaveRequestRecordRules
{
    public static LegacyLeaveRequestRecordValidator DefaultLegacyLeaveRequestRecordValidator()
    {
        return new LegacyLeaveRequestRecordValidator(
            new List<ILegacyLeaveRequestRecordRule>
            {
                new LegacyLeaveRequestRecordRequiredNameRule("legacy leave request record needs this field"),
                new LegacyLeaveRequestRecordExpiresAtRangeRule(255, 1000),
                new LegacyLeaveRequestRecordRatioRangeRule(7.0, 2230.8),
                new LegacyLeaveRequestRecordCodeLengthRule(80),
            });
    }
}
