namespace Acme.Workforce.Compliance.Certification.Review;

/// <summary>A rule a default certification budget must pass. Check returns its problems, or none.</summary>
public interface IDefaultCertificationBudgetRule
{
    List<string> Check(DefaultCertificationBudget item);
}

public sealed record DefaultCertificationBudgetRequiredCodeRule(string Message) : IDefaultCertificationBudgetRule
{
    public List<string> Check(DefaultCertificationBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultCertificationBudgetExpiresAtRangeRule(int Min, int Max) : IDefaultCertificationBudgetRule
{
    public List<string> Check(DefaultCertificationBudget item)
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

public sealed record DefaultCertificationBudgetRequiredTeamRule(string Message) : IDefaultCertificationBudgetRule
{
    public List<string> Check(DefaultCertificationBudget item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultCertificationBudgetLabelLengthRule(int MaxLength) : IDefaultCertificationBudgetRule
{
    public List<string> Check(DefaultCertificationBudget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default certification budget.</summary>
public sealed record DefaultCertificationBudgetValidator(List<IDefaultCertificationBudgetRule> Rules)
{
    public List<string> Validate(DefaultCertificationBudget item)
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

    public void RequireValid(DefaultCertificationBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultCertificationBudget> items)
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

/// <summary>Functions of default certification budget rules.</summary>
public static class DefaultCertificationBudgetRules
{
    public static DefaultCertificationBudgetValidator DefaultDefaultCertificationBudgetValidator()
    {
        return new DefaultCertificationBudgetValidator(
            new List<IDefaultCertificationBudgetRule>
            {
                new DefaultCertificationBudgetRequiredCodeRule("default certification budget needs a value"),
                new DefaultCertificationBudgetExpiresAtRangeRule(64, 86400),
                new DefaultCertificationBudgetRequiredTeamRule("default certification budget needs a value"),
                new DefaultCertificationBudgetLabelLengthRule(86400),
            });
    }
}
