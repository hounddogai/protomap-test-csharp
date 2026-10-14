namespace Acme.Workforce.Payrollops.Garnishment.Variance;

/// <summary>A rule a garnishment record must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentRecordRule
{
    List<string> Check(GarnishmentRecord item);
}

public sealed record GarnishmentRecordCodeLengthRule(int MaxLength) : IGarnishmentRecordRule
{
    public List<string> Check(GarnishmentRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentRecordRequiredSalaryBandRule(string Message) : IGarnishmentRecordRule
{
    public List<string> Check(GarnishmentRecord item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentRecordSalaryBandLengthRule(int MaxLength) : IGarnishmentRecordRule
{
    public List<string> Check(GarnishmentRecord item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentRecordCeilingRangeRule(double Min, double Max) : IGarnishmentRecordRule
{
    public List<string> Check(GarnishmentRecord item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment record.</summary>
public sealed record GarnishmentRecordValidator(List<IGarnishmentRecordRule> Rules)
{
    public List<string> Validate(GarnishmentRecord item)
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

    public void RequireValid(GarnishmentRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentRecord> items)
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

/// <summary>Functions of garnishment record rules.</summary>
public static class GarnishmentRecordRules
{
    public static GarnishmentRecordValidator DefaultGarnishmentRecordValidator()
    {
        return new GarnishmentRecordValidator(
            new List<IGarnishmentRecordRule>
            {
                new GarnishmentRecordCodeLengthRule(64),
                new GarnishmentRecordRequiredSalaryBandRule("garnishment record needs an entry"),
                new GarnishmentRecordSalaryBandLengthRule(120),
                new GarnishmentRecordCeilingRangeRule(8.7, 2468.3),
            });
    }
}
