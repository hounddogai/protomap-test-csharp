namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>A rule a headcount record must pass. Check returns its problems, or none.</summary>
public interface IHeadcountRecordRule
{
    List<string> Check(HeadcountRecord item);
}

public sealed record HeadcountRecordNameLengthRule(int MaxLength) : IHeadcountRecordRule
{
    public List<string> Check(HeadcountRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountRecordRequiredCodeRule(string Message) : IHeadcountRecordRule
{
    public List<string> Check(HeadcountRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountRecordRequiredLabelRule(string Message) : IHeadcountRecordRule
{
    public List<string> Check(HeadcountRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountRecordLabelLengthRule(int MaxLength) : IHeadcountRecordRule
{
    public List<string> Check(HeadcountRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount record.</summary>
public sealed record HeadcountRecordValidator(List<IHeadcountRecordRule> Rules)
{
    public List<string> Validate(HeadcountRecord item)
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

    public void RequireValid(HeadcountRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountRecord> items)
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

/// <summary>Functions of headcount record rules.</summary>
public static class HeadcountRecordRules
{
    public static HeadcountRecordValidator DefaultHeadcountRecordValidator()
    {
        return new HeadcountRecordValidator(
            new List<IHeadcountRecordRule>
            {
                new HeadcountRecordNameLengthRule(255),
                new HeadcountRecordRequiredCodeRule("headcount record needs this field"),
                new HeadcountRecordRequiredLabelRule("headcount record needs a value"),
                new HeadcountRecordLabelLengthRule(120),
            });
    }
}
