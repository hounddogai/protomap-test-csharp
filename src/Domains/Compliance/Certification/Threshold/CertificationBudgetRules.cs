namespace Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>A rule a certification budget must pass. Check returns its problems, or none.</summary>
public interface ICertificationBudgetRule
{
    List<string> Check(CertificationBudget item);
}

public sealed record CertificationBudgetRequiredChannelRule(string Message) : ICertificationBudgetRule
{
    public List<string> Check(CertificationBudget item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationBudgetRequiredTitleRule(string Message) : ICertificationBudgetRule
{
    public List<string> Check(CertificationBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationBudgetRequiredCodeRule(string Message) : ICertificationBudgetRule
{
    public List<string> Check(CertificationBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification budget.</summary>
public sealed record CertificationBudgetValidator(List<ICertificationBudgetRule> Rules)
{
    public List<string> Validate(CertificationBudget item)
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

    public void RequireValid(CertificationBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationBudget> items)
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

/// <summary>Functions of certification budget rules.</summary>
public static class CertificationBudgetRules
{
    public static CertificationBudgetValidator DefaultCertificationBudgetValidator()
    {
        return new CertificationBudgetValidator(
            new List<ICertificationBudgetRule>
            {
                new CertificationBudgetRequiredChannelRule("certification budget needs this field"),
                new CertificationBudgetRequiredTitleRule("certification budget needs a value"),
                new CertificationBudgetRequiredCodeRule("certification budget needs an entry"),
            });
    }
}
