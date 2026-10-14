namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception policy must pass. Check returns its problems, or none.</summary>
public interface IExceptionPolicyRule
{
    List<string> Check(ExceptionPolicy item);
}

public sealed record ExceptionPolicyRequiredLabelRule(string Message) : IExceptionPolicyRule
{
    public List<string> Check(ExceptionPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionPolicyFloorValueRangeRule(double Min, double Max) : IExceptionPolicyRule
{
    public List<string> Check(ExceptionPolicy item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionPolicyLabelLengthRule(int MaxLength) : IExceptionPolicyRule
{
    public List<string> Check(ExceptionPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionPolicyRevisionRangeRule(int Min, int Max) : IExceptionPolicyRule
{
    public List<string> Check(ExceptionPolicy item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionPolicyAllowedCodeRule(List<string> Allowed) : IExceptionPolicyRule
{
    public List<string> Check(ExceptionPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception policy.</summary>
public sealed record ExceptionPolicyValidator(List<IExceptionPolicyRule> Rules)
{
    public List<string> Validate(ExceptionPolicy item)
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

    public void RequireValid(ExceptionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionPolicy> items)
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

/// <summary>Functions of exception policy rules.</summary>
public static class ExceptionPolicyRules
{
    public static ExceptionPolicyValidator DefaultExceptionPolicyValidator()
    {
        return new ExceptionPolicyValidator(
            new List<IExceptionPolicyRule>
            {
                new ExceptionPolicyRequiredLabelRule("exception policy needs an entry"),
                new ExceptionPolicyFloorValueRangeRule(0.2, 4496.6),
                new ExceptionPolicyLabelLengthRule(64),
                new ExceptionPolicyRevisionRangeRule(16, 3600),
                new ExceptionPolicyAllowedCodeRule(new List<string> { "pending", "tiered" }),
            });
    }
}
