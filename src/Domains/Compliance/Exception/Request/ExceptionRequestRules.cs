namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception request must pass. Check returns its problems, or none.</summary>
public interface IExceptionRequestRule
{
    List<string> Check(ExceptionRequest item);
}

public sealed record ExceptionRequestRequiredExternalRefRule(string Message) : IExceptionRequestRule
{
    public List<string> Check(ExceptionRequest item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRequestAllowedLabelRule(List<string> Allowed) : IExceptionRequestRule
{
    public List<string> Check(ExceptionRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRequestVisibleGuardRule(double Limit) : IExceptionRequestRule
{
    public List<string> Check(ExceptionRequest item)
    {
        if (item.Visible && item.Rating > Limit)
        {
            return new List<string> { $"rating {item.Rating} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRequestRequiredCodeRule(string Message) : IExceptionRequestRule
{
    public List<string> Check(ExceptionRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception request.</summary>
public sealed record ExceptionRequestValidator(List<IExceptionRequestRule> Rules)
{
    public List<string> Validate(ExceptionRequest item)
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

    public void RequireValid(ExceptionRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionRequest> items)
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

/// <summary>Functions of exception request rules.</summary>
public static class ExceptionRequestRules
{
    public static ExceptionRequestValidator DefaultExceptionRequestValidator()
    {
        return new ExceptionRequestValidator(
            new List<IExceptionRequestRule>
            {
                new ExceptionRequestRequiredExternalRefRule("exception request needs a value"),
                new ExceptionRequestAllowedLabelRule(new List<string> { "capped", "seasonal", "legacy" }),
                new ExceptionRequestVisibleGuardRule(5.1),
                new ExceptionRequestRequiredCodeRule("exception request needs an entry"),
            });
    }
}
