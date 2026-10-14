namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount budget must pass. Check returns its problems, or none.</summary>
public interface IHeadcountBudgetRule
{
    List<string> Check(HeadcountBudget item);
}

public sealed record HeadcountBudgetTitleLengthRule(int MaxLength) : IHeadcountBudgetRule
{
    public List<string> Check(HeadcountBudget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBudgetRequiredPhoneRule(string Message) : IHeadcountBudgetRule
{
    public List<string> Check(HeadcountBudget item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBudgetAllowedPhoneRule(List<string> Allowed) : IHeadcountBudgetRule
{
    public List<string> Check(HeadcountBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is expired" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBudgetRequiredCodeRule(string Message) : IHeadcountBudgetRule
{
    public List<string> Check(HeadcountBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBudgetPhoneLengthRule(int MaxLength) : IHeadcountBudgetRule
{
    public List<string> Check(HeadcountBudget item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount budget.</summary>
public sealed record HeadcountBudgetValidator(List<IHeadcountBudgetRule> Rules)
{
    public List<string> Validate(HeadcountBudget item)
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

    public void RequireValid(HeadcountBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountBudget> items)
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

/// <summary>Functions of headcount budget rules.</summary>
public static class HeadcountBudgetRules
{
    public static HeadcountBudgetValidator DefaultHeadcountBudgetValidator()
    {
        return new HeadcountBudgetValidator(
            new List<IHeadcountBudgetRule>
            {
                new HeadcountBudgetTitleLengthRule(86400),
                new HeadcountBudgetRequiredPhoneRule("headcount budget needs a value"),
                new HeadcountBudgetAllowedPhoneRule(new List<string> { "lenient", "adaptive" }),
                new HeadcountBudgetRequiredCodeRule("headcount budget needs this field"),
                new HeadcountBudgetPhoneLengthRule(16),
            });
    }
}
