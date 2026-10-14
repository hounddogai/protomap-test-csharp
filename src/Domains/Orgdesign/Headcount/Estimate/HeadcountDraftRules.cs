namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

/// <summary>A rule a headcount draft must pass. Check returns its problems, or none.</summary>
public interface IHeadcountDraftRule
{
    List<string> Check(HeadcountDraft item);
}

public sealed record HeadcountDraftContactEmailLengthRule(int MaxLength) : IHeadcountDraftRule
{
    public List<string> Check(HeadcountDraft item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountDraftRequiredTitleRule(string Message) : IHeadcountDraftRule
{
    public List<string> Check(HeadcountDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountDraftRequiredContactEmailRule(string Message) : IHeadcountDraftRule
{
    public List<string> Check(HeadcountDraft item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountDraftRequiredCodeRule(string Message) : IHeadcountDraftRule
{
    public List<string> Check(HeadcountDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountDraftLimitRangeRule(int Min, int Max) : IHeadcountDraftRule
{
    public List<string> Check(HeadcountDraft item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountDraftArchivedGuardRule(int Limit) : IHeadcountDraftRule
{
    public List<string> Check(HeadcountDraft item)
    {
        if (item.Archived && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount draft.</summary>
public sealed record HeadcountDraftValidator(List<IHeadcountDraftRule> Rules)
{
    public List<string> Validate(HeadcountDraft item)
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

    public void RequireValid(HeadcountDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountDraft> items)
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

/// <summary>Functions of headcount draft rules.</summary>
public static class HeadcountDraftRules
{
    public static HeadcountDraftValidator DefaultHeadcountDraftValidator()
    {
        return new HeadcountDraftValidator(
            new List<IHeadcountDraftRule>
            {
                new HeadcountDraftContactEmailLengthRule(120),
                new HeadcountDraftRequiredTitleRule("headcount draft needs this field"),
                new HeadcountDraftRequiredContactEmailRule("headcount draft needs a value"),
                new HeadcountDraftRequiredCodeRule("headcount draft needs a value"),
                new HeadcountDraftLimitRangeRule(120, 255),
                new HeadcountDraftArchivedGuardRule(1000),
            });
    }
}
