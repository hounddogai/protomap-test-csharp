namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh rollup must pass. Check returns its problems, or none.</summary>
public interface IRefreshRollupRule
{
    List<string> Check(RefreshRollup item);
}

public sealed record RefreshRollupAllowedCodeRule(List<string> Allowed) : IRefreshRollupRule
{
    public List<string> Check(RefreshRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRollupLabelLengthRule(int MaxLength) : IRefreshRollupRule
{
    public List<string> Check(RefreshRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRollupCodeLengthRule(int MaxLength) : IRefreshRollupRule
{
    public List<string> Check(RefreshRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRollupRequiredEmployeeIdRule(string Message) : IRefreshRollupRule
{
    public List<string> Check(RefreshRollup item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh rollup.</summary>
public sealed record RefreshRollupValidator(List<IRefreshRollupRule> Rules)
{
    public List<string> Validate(RefreshRollup item)
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

    public void RequireValid(RefreshRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshRollup> items)
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

/// <summary>Functions of refresh rollup rules.</summary>
public static class RefreshRollupRules
{
    public static RefreshRollupValidator DefaultRefreshRollupValidator()
    {
        return new RefreshRollupValidator(
            new List<IRefreshRollupRule>
            {
                new RefreshRollupAllowedCodeRule(new List<string>()),
                new RefreshRollupLabelLengthRule(32),
                new RefreshRollupCodeLengthRule(64),
                new RefreshRollupRequiredEmployeeIdRule("refresh rollup needs this field"),
            });
    }
}
