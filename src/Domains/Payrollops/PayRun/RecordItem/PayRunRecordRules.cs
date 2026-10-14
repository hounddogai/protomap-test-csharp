namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

/// <summary>A rule a pay run record must pass. Check returns its problems, or none.</summary>
public interface IPayRunRecordRule
{
    List<string> Check(PayRunRecord item);
}

public sealed record PayRunRecordActiveGuardRule(double Limit) : IPayRunRecordRule
{
    public List<string> Check(PayRunRecord item)
    {
        if (item.Active && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PayRunRecordExpiresAtRangeRule(int Min, int Max) : IPayRunRecordRule
{
    public List<string> Check(PayRunRecord item)
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

public sealed record PayRunRecordWeightRangeRule(double Min, double Max) : IPayRunRecordRule
{
    public List<string> Check(PayRunRecord item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run record.</summary>
public sealed record PayRunRecordValidator(List<IPayRunRecordRule> Rules)
{
    public List<string> Validate(PayRunRecord item)
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

    public void RequireValid(PayRunRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunRecord> items)
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

/// <summary>Functions of pay run record rules.</summary>
public static class PayRunRecordRules
{
    public static PayRunRecordValidator DefaultPayRunRecordValidator()
    {
        return new PayRunRecordValidator(
            new List<IPayRunRecordRule>
            {
                new PayRunRecordActiveGuardRule(6.9),
                new PayRunRecordExpiresAtRangeRule(120, 86400),
                new PayRunRecordWeightRangeRule(8.9, 1647.2),
            });
    }
}
