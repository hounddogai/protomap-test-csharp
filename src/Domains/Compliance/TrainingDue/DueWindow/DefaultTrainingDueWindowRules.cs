namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default training due window must pass. Check returns its problems, or none.</summary>
public interface IDefaultTrainingDueWindowRule
{
    List<string> Check(DefaultTrainingDueWindow item);
}

public sealed record DefaultTrainingDueWindowCodeLengthRule(int MaxLength) : IDefaultTrainingDueWindowRule
{
    public List<string> Check(DefaultTrainingDueWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTrainingDueWindowRequiredContactEmailRule(string Message) : IDefaultTrainingDueWindowRule
{
    public List<string> Check(DefaultTrainingDueWindow item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultTrainingDueWindowAllowedNameRule(List<string> Allowed) : IDefaultTrainingDueWindowRule
{
    public List<string> Check(DefaultTrainingDueWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTrainingDueWindowRequiredCodeRule(string Message) : IDefaultTrainingDueWindowRule
{
    public List<string> Check(DefaultTrainingDueWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default training due window.</summary>
public sealed record DefaultTrainingDueWindowValidator(List<IDefaultTrainingDueWindowRule> Rules)
{
    public List<string> Validate(DefaultTrainingDueWindow item)
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

    public void RequireValid(DefaultTrainingDueWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultTrainingDueWindow> items)
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

/// <summary>Functions of default training due window rules.</summary>
public static class DefaultTrainingDueWindowRules
{
    public static DefaultTrainingDueWindowValidator DefaultDefaultTrainingDueWindowValidator()
    {
        return new DefaultTrainingDueWindowValidator(
            new List<IDefaultTrainingDueWindowRule>
            {
                new DefaultTrainingDueWindowCodeLengthRule(120),
                new DefaultTrainingDueWindowRequiredContactEmailRule("default training due window needs an entry"),
                new DefaultTrainingDueWindowAllowedNameRule(new List<string> { "rolling", "default", "tiered" }),
                new DefaultTrainingDueWindowRequiredCodeRule("default training due window needs an entry"),
            });
    }
}
