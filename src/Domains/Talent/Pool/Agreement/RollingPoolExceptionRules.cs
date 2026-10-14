namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling pool exception must pass. Check returns its problems, or none.</summary>
public interface IRollingPoolExceptionRule
{
    List<string> Check(RollingPoolException item);
}

public sealed record RollingPoolExceptionAllowedExternalRefRule(List<string> Allowed) : IRollingPoolExceptionRule
{
    public List<string> Check(RollingPoolException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record RollingPoolExceptionCodeLengthRule(int MaxLength) : IRollingPoolExceptionRule
{
    public List<string> Check(RollingPoolException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPoolExceptionRequiredCodeRule(string Message) : IRollingPoolExceptionRule
{
    public List<string> Check(RollingPoolException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPoolExceptionLabelLengthRule(int MaxLength) : IRollingPoolExceptionRule
{
    public List<string> Check(RollingPoolException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPoolExceptionLeaveDaysRangeRule(int Min, int Max) : IRollingPoolExceptionRule
{
    public List<string> Check(RollingPoolException item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pool exception.</summary>
public sealed record RollingPoolExceptionValidator(List<IRollingPoolExceptionRule> Rules)
{
    public List<string> Validate(RollingPoolException item)
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

    public void RequireValid(RollingPoolException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPoolException> items)
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

/// <summary>Functions of rolling pool exception rules.</summary>
public static class RollingPoolExceptionRules
{
    public static RollingPoolExceptionValidator DefaultRollingPoolExceptionValidator()
    {
        return new RollingPoolExceptionValidator(
            new List<IRollingPoolExceptionRule>
            {
                new RollingPoolExceptionAllowedExternalRefRule(new List<string> { "regional" }),
                new RollingPoolExceptionCodeLengthRule(3600),
                new RollingPoolExceptionRequiredCodeRule("rolling pool exception needs this field"),
                new RollingPoolExceptionLabelLengthRule(120),
                new RollingPoolExceptionLeaveDaysRangeRule(80, 255),
            });
    }
}
