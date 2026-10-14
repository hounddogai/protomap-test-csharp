namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global position exception must pass. Check returns its problems, or none.</summary>
public interface IGlobalPositionExceptionRule
{
    List<string> Check(GlobalPositionException item);
}

public sealed record GlobalPositionExceptionAllowedGradeRule(List<string> Allowed) : IGlobalPositionExceptionRule
{
    public List<string> Check(GlobalPositionException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is stale" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPositionExceptionGradeLengthRule(int MaxLength) : IGlobalPositionExceptionRule
{
    public List<string> Check(GlobalPositionException item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPositionExceptionPriorityRangeRule(int Min, int Max) : IGlobalPositionExceptionRule
{
    public List<string> Check(GlobalPositionException item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPositionExceptionRequiredCodeRule(string Message) : IGlobalPositionExceptionRule
{
    public List<string> Check(GlobalPositionException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global position exception.</summary>
public sealed record GlobalPositionExceptionValidator(List<IGlobalPositionExceptionRule> Rules)
{
    public List<string> Validate(GlobalPositionException item)
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

    public void RequireValid(GlobalPositionException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPositionException> items)
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

/// <summary>Functions of global position exception rules.</summary>
public static class GlobalPositionExceptionRules
{
    public static GlobalPositionExceptionValidator DefaultGlobalPositionExceptionValidator()
    {
        return new GlobalPositionExceptionValidator(
            new List<IGlobalPositionExceptionRule>
            {
                new GlobalPositionExceptionAllowedGradeRule(new List<string> { "scheduled", "manual", "weighted" }),
                new GlobalPositionExceptionGradeLengthRule(120),
                new GlobalPositionExceptionPriorityRangeRule(32, 86400),
                new GlobalPositionExceptionRequiredCodeRule("global position exception needs a value"),
            });
    }
}
