namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment exception must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentExceptionRule
{
    List<string> Check(GarnishmentException item);
}

public sealed record GarnishmentExceptionCostRangeRule(double Min, double Max) : IGarnishmentExceptionRule
{
    public List<string> Check(GarnishmentException item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentExceptionAllowedCodeRule(List<string> Allowed) : IGarnishmentExceptionRule
{
    public List<string> Check(GarnishmentException item)
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

public sealed record GarnishmentExceptionRequiredTitleRule(string Message) : IGarnishmentExceptionRule
{
    public List<string> Check(GarnishmentException item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentExceptionGradeLengthRule(int MaxLength) : IGarnishmentExceptionRule
{
    public List<string> Check(GarnishmentException item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentExceptionRequiredGradeRule(string Message) : IGarnishmentExceptionRule
{
    public List<string> Check(GarnishmentException item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentExceptionCodeLengthRule(int MaxLength) : IGarnishmentExceptionRule
{
    public List<string> Check(GarnishmentException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment exception.</summary>
public sealed record GarnishmentExceptionValidator(List<IGarnishmentExceptionRule> Rules)
{
    public List<string> Validate(GarnishmentException item)
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

    public void RequireValid(GarnishmentException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentException> items)
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

/// <summary>Functions of garnishment exception rules.</summary>
public static class GarnishmentExceptionRules
{
    public static GarnishmentExceptionValidator DefaultGarnishmentExceptionValidator()
    {
        return new GarnishmentExceptionValidator(
            new List<IGarnishmentExceptionRule>
            {
                new GarnishmentExceptionCostRangeRule(1.1, 4602.5),
                new GarnishmentExceptionAllowedCodeRule(new List<string> { "batched" }),
                new GarnishmentExceptionRequiredTitleRule("garnishment exception needs an entry"),
                new GarnishmentExceptionGradeLengthRule(255),
                new GarnishmentExceptionRequiredGradeRule("garnishment exception needs an entry"),
                new GarnishmentExceptionCodeLengthRule(16),
            });
    }
}
