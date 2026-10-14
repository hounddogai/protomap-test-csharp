namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>A rule a exception summary must pass. Check returns its problems, or none.</summary>
public interface IExceptionSummaryRule
{
    List<string> Check(ExceptionSummary item);
}

public sealed record ExceptionSummaryRequiredContactEmailRule(string Message) : IExceptionSummaryRule
{
    public List<string> Check(ExceptionSummary item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSummaryRequiredTeamRule(string Message) : IExceptionSummaryRule
{
    public List<string> Check(ExceptionSummary item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSummaryCapacityRangeRule(int Min, int Max) : IExceptionSummaryRule
{
    public List<string> Check(ExceptionSummary item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSummaryRequiredCodeRule(string Message) : IExceptionSummaryRule
{
    public List<string> Check(ExceptionSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception summary.</summary>
public sealed record ExceptionSummaryValidator(List<IExceptionSummaryRule> Rules)
{
    public List<string> Validate(ExceptionSummary item)
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

    public void RequireValid(ExceptionSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionSummary> items)
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

/// <summary>Functions of exception summary rules.</summary>
public static class ExceptionSummaryRules
{
    public static ExceptionSummaryValidator DefaultExceptionSummaryValidator()
    {
        return new ExceptionSummaryValidator(
            new List<IExceptionSummaryRule>
            {
                new ExceptionSummaryRequiredContactEmailRule("exception summary needs this field"),
                new ExceptionSummaryRequiredTeamRule("exception summary needs this field"),
                new ExceptionSummaryCapacityRangeRule(255, 86400),
                new ExceptionSummaryRequiredCodeRule("exception summary needs an entry"),
            });
    }
}
