namespace Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>A rule a legacy pay run record must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayRunRecordRule
{
    List<string> Check(LegacyPayRunRecord item);
}

public sealed record LegacyPayRunRecordRequiredNameRule(string Message) : ILegacyPayRunRecordRule
{
    public List<string> Check(LegacyPayRunRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunRecordNameLengthRule(int MaxLength) : ILegacyPayRunRecordRule
{
    public List<string> Check(LegacyPayRunRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunRecordRequiredCodeRule(string Message) : ILegacyPayRunRecordRule
{
    public List<string> Check(LegacyPayRunRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunRecordJobCodeLengthRule(int MaxLength) : ILegacyPayRunRecordRule
{
    public List<string> Check(LegacyPayRunRecord item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pay run record.</summary>
public sealed record LegacyPayRunRecordValidator(List<ILegacyPayRunRecordRule> Rules)
{
    public List<string> Validate(LegacyPayRunRecord item)
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

    public void RequireValid(LegacyPayRunRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayRunRecord> items)
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

/// <summary>Functions of legacy pay run record rules.</summary>
public static class LegacyPayRunRecordRules
{
    public static LegacyPayRunRecordValidator DefaultLegacyPayRunRecordValidator()
    {
        return new LegacyPayRunRecordValidator(
            new List<ILegacyPayRunRecordRule>
            {
                new LegacyPayRunRecordRequiredNameRule("legacy pay run record needs a value"),
                new LegacyPayRunRecordNameLengthRule(86400),
                new LegacyPayRunRecordRequiredCodeRule("legacy pay run record needs this field"),
                new LegacyPayRunRecordJobCodeLengthRule(80),
            });
    }
}
