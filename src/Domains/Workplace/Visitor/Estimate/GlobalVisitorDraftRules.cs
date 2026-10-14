namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global visitor draft must pass. Check returns its problems, or none.</summary>
public interface IGlobalVisitorDraftRule
{
    List<string> Check(GlobalVisitorDraft item);
}

public sealed record GlobalVisitorDraftAllowedCountryRule(List<string> Allowed) : IGlobalVisitorDraftRule
{
    public List<string> Check(GlobalVisitorDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record GlobalVisitorDraftTenureMonthsRangeRule(int Min, int Max) : IGlobalVisitorDraftRule
{
    public List<string> Check(GlobalVisitorDraft item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalVisitorDraftRequiredCodeRule(string Message) : IGlobalVisitorDraftRule
{
    public List<string> Check(GlobalVisitorDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalVisitorDraftRequiredTitleRule(string Message) : IGlobalVisitorDraftRule
{
    public List<string> Check(GlobalVisitorDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalVisitorDraftRequiredLocationRule(string Message) : IGlobalVisitorDraftRule
{
    public List<string> Check(GlobalVisitorDraft item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global visitor draft.</summary>
public sealed record GlobalVisitorDraftValidator(List<IGlobalVisitorDraftRule> Rules)
{
    public List<string> Validate(GlobalVisitorDraft item)
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

    public void RequireValid(GlobalVisitorDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalVisitorDraft> items)
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

/// <summary>Functions of global visitor draft rules.</summary>
public static class GlobalVisitorDraftRules
{
    public static GlobalVisitorDraftValidator DefaultGlobalVisitorDraftValidator()
    {
        return new GlobalVisitorDraftValidator(
            new List<IGlobalVisitorDraftRule>
            {
                new GlobalVisitorDraftAllowedCountryRule(new List<string> { "manual", "rolling", "seasonal" }),
                new GlobalVisitorDraftTenureMonthsRangeRule(16, 255),
                new GlobalVisitorDraftRequiredCodeRule("global visitor draft needs a value"),
                new GlobalVisitorDraftRequiredTitleRule("global visitor draft needs a value"),
                new GlobalVisitorDraftRequiredLocationRule("global visitor draft needs an entry"),
            });
    }
}
