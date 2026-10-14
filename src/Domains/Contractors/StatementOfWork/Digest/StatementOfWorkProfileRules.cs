namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work profile must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkProfileRule
{
    List<string> Check(StatementOfWorkProfile item);
}

public sealed record StatementOfWorkProfileThresholdRangeRule(double Min, double Max) : IStatementOfWorkProfileRule
{
    public List<string> Check(StatementOfWorkProfile item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkProfileAllowedBadgeRule(List<string> Allowed) : IStatementOfWorkProfileRule
{
    public List<string> Check(StatementOfWorkProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkProfileExpiresAtRangeRule(int Min, int Max) : IStatementOfWorkProfileRule
{
    public List<string> Check(StatementOfWorkProfile item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkProfileRequiredBadgeRule(string Message) : IStatementOfWorkProfileRule
{
    public List<string> Check(StatementOfWorkProfile item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work profile.</summary>
public sealed record StatementOfWorkProfileValidator(List<IStatementOfWorkProfileRule> Rules)
{
    public List<string> Validate(StatementOfWorkProfile item)
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

    public void RequireValid(StatementOfWorkProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkProfile> items)
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

/// <summary>Functions of statement of work profile rules.</summary>
public static class StatementOfWorkProfileRules
{
    public static StatementOfWorkProfileValidator DefaultStatementOfWorkProfileValidator()
    {
        return new StatementOfWorkProfileValidator(
            new List<IStatementOfWorkProfileRule>
            {
                new StatementOfWorkProfileThresholdRangeRule(9.1, 4021.2),
                new StatementOfWorkProfileAllowedBadgeRule(new List<string> { "weighted", "default" }),
                new StatementOfWorkProfileExpiresAtRangeRule(64, 80),
                new StatementOfWorkProfileRequiredBadgeRule("statement of work profile needs a value"),
            });
    }
}
