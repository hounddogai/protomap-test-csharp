namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict exception record must pass. Check returns its problems, or none.</summary>
public interface IStrictExceptionRecordRule
{
    List<string> Check(StrictExceptionRecord item);
}

public sealed record StrictExceptionRecordRequiredCodeRule(string Message) : IStrictExceptionRecordRule
{
    public List<string> Check(StrictExceptionRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictExceptionRecordSalaryBandLengthRule(int MaxLength) : IStrictExceptionRecordRule
{
    public List<string> Check(StrictExceptionRecord item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictExceptionRecordTitleLengthRule(int MaxLength) : IStrictExceptionRecordRule
{
    public List<string> Check(StrictExceptionRecord item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictExceptionRecordVersionRangeRule(int Min, int Max) : IStrictExceptionRecordRule
{
    public List<string> Check(StrictExceptionRecord item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictExceptionRecordAllowedSalaryBandRule(List<string> Allowed) : IStrictExceptionRecordRule
{
    public List<string> Check(StrictExceptionRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record StrictExceptionRecordRequiredSalaryBandRule(string Message) : IStrictExceptionRecordRule
{
    public List<string> Check(StrictExceptionRecord item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict exception record.</summary>
public sealed record StrictExceptionRecordValidator(List<IStrictExceptionRecordRule> Rules)
{
    public List<string> Validate(StrictExceptionRecord item)
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

    public void RequireValid(StrictExceptionRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictExceptionRecord> items)
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

/// <summary>Functions of strict exception record rules.</summary>
public static class StrictExceptionRecordRules
{
    public static StrictExceptionRecordValidator DefaultStrictExceptionRecordValidator()
    {
        return new StrictExceptionRecordValidator(
            new List<IStrictExceptionRecordRule>
            {
                new StrictExceptionRecordRequiredCodeRule("strict exception record needs this field"),
                new StrictExceptionRecordSalaryBandLengthRule(120),
                new StrictExceptionRecordTitleLengthRule(120),
                new StrictExceptionRecordVersionRangeRule(120, 3600),
                new StrictExceptionRecordAllowedSalaryBandRule(new List<string>()),
                new StrictExceptionRecordRequiredSalaryBandRule("strict exception record needs this field"),
            });
    }
}
