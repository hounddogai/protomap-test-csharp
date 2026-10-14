namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

/// <summary>A rule a capped garnishment record must pass. Check returns its problems, or none.</summary>
public interface ICappedGarnishmentRecordRule
{
    List<string> Check(CappedGarnishmentRecord item);
}

public sealed record CappedGarnishmentRecordStartsAtRangeRule(int Min, int Max) : ICappedGarnishmentRecordRule
{
    public List<string> Check(CappedGarnishmentRecord item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedGarnishmentRecordApprovedGuardRule(int Limit) : ICappedGarnishmentRecordRule
{
    public List<string> Check(CappedGarnishmentRecord item)
    {
        if (item.Approved && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record CappedGarnishmentRecordRequiredNameRule(string Message) : ICappedGarnishmentRecordRule
{
    public List<string> Check(CappedGarnishmentRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped garnishment record.</summary>
public sealed record CappedGarnishmentRecordValidator(List<ICappedGarnishmentRecordRule> Rules)
{
    public List<string> Validate(CappedGarnishmentRecord item)
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

    public void RequireValid(CappedGarnishmentRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedGarnishmentRecord> items)
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

/// <summary>Functions of capped garnishment record rules.</summary>
public static class CappedGarnishmentRecordRules
{
    public static CappedGarnishmentRecordValidator DefaultCappedGarnishmentRecordValidator()
    {
        return new CappedGarnishmentRecordValidator(
            new List<ICappedGarnishmentRecordRule>
            {
                new CappedGarnishmentRecordStartsAtRangeRule(32, 255),
                new CappedGarnishmentRecordApprovedGuardRule(120),
                new CappedGarnishmentRecordRequiredNameRule("capped garnishment record needs a value"),
            });
    }
}
