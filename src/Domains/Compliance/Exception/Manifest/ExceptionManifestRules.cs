namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception manifest must pass. Check returns its problems, or none.</summary>
public interface IExceptionManifestRule
{
    List<string> Check(ExceptionManifest item);
}

public sealed record ExceptionManifestAllowedEmployeeIdRule(List<string> Allowed) : IExceptionManifestRule
{
    public List<string> Check(ExceptionManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionManifestRequiredTitleRule(string Message) : IExceptionManifestRule
{
    public List<string> Check(ExceptionManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionManifestRequiredEmployeeIdRule(string Message) : IExceptionManifestRule
{
    public List<string> Check(ExceptionManifest item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception manifest.</summary>
public sealed record ExceptionManifestValidator(List<IExceptionManifestRule> Rules)
{
    public List<string> Validate(ExceptionManifest item)
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

    public void RequireValid(ExceptionManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionManifest> items)
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

/// <summary>Functions of exception manifest rules.</summary>
public static class ExceptionManifestRules
{
    public static ExceptionManifestValidator DefaultExceptionManifestValidator()
    {
        return new ExceptionManifestValidator(
            new List<IExceptionManifestRule>
            {
                new ExceptionManifestAllowedEmployeeIdRule(new List<string> { "pending" }),
                new ExceptionManifestRequiredTitleRule("exception manifest needs an entry"),
                new ExceptionManifestRequiredEmployeeIdRule("exception manifest needs an entry"),
            });
    }
}
