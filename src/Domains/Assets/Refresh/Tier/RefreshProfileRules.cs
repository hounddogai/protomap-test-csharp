namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh profile must pass. Check returns its problems, or none.</summary>
public interface IRefreshProfileRule
{
    List<string> Check(RefreshProfile item);
}

public sealed record RefreshProfileRequiredTitleRule(string Message) : IRefreshProfileRule
{
    public List<string> Check(RefreshProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshProfileRequiredGradeRule(string Message) : IRefreshProfileRule
{
    public List<string> Check(RefreshProfile item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshProfileAllowedTitleRule(List<string> Allowed) : IRefreshProfileRule
{
    public List<string> Check(RefreshProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record RefreshProfileCodeLengthRule(int MaxLength) : IRefreshProfileRule
{
    public List<string> Check(RefreshProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshProfileGradeLengthRule(int MaxLength) : IRefreshProfileRule
{
    public List<string> Check(RefreshProfile item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh profile.</summary>
public sealed record RefreshProfileValidator(List<IRefreshProfileRule> Rules)
{
    public List<string> Validate(RefreshProfile item)
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

    public void RequireValid(RefreshProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshProfile> items)
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

/// <summary>Functions of refresh profile rules.</summary>
public static class RefreshProfileRules
{
    public static RefreshProfileValidator DefaultRefreshProfileValidator()
    {
        return new RefreshProfileValidator(
            new List<IRefreshProfileRule>
            {
                new RefreshProfileRequiredTitleRule("refresh profile needs an entry"),
                new RefreshProfileRequiredGradeRule("refresh profile needs an entry"),
                new RefreshProfileAllowedTitleRule(new List<string> { "tiered" }),
                new RefreshProfileCodeLengthRule(1000),
                new RefreshProfileGradeLengthRule(255),
            });
    }
}
