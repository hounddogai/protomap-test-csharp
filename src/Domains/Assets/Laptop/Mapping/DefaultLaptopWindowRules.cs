namespace Acme.Workforce.Assets.Laptop.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default laptop window must pass. Check returns its problems, or none.</summary>
public interface IDefaultLaptopWindowRule
{
    List<string> Check(DefaultLaptopWindow item);
}

public sealed record DefaultLaptopWindowGradeLengthRule(int MaxLength) : IDefaultLaptopWindowRule
{
    public List<string> Check(DefaultLaptopWindow item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopWindowAllowedGradeRule(List<string> Allowed) : IDefaultLaptopWindowRule
{
    public List<string> Check(DefaultLaptopWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopWindowRequiredGradeRule(string Message) : IDefaultLaptopWindowRule
{
    public List<string> Check(DefaultLaptopWindow item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopWindowLabelLengthRule(int MaxLength) : IDefaultLaptopWindowRule
{
    public List<string> Check(DefaultLaptopWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopWindowRequiredLabelRule(string Message) : IDefaultLaptopWindowRule
{
    public List<string> Check(DefaultLaptopWindow item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopWindowCodeLengthRule(int MaxLength) : IDefaultLaptopWindowRule
{
    public List<string> Check(DefaultLaptopWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default laptop window.</summary>
public sealed record DefaultLaptopWindowValidator(List<IDefaultLaptopWindowRule> Rules)
{
    public List<string> Validate(DefaultLaptopWindow item)
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

    public void RequireValid(DefaultLaptopWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLaptopWindow> items)
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

/// <summary>Functions of default laptop window rules.</summary>
public static class DefaultLaptopWindowRules
{
    public static DefaultLaptopWindowValidator DefaultDefaultLaptopWindowValidator()
    {
        return new DefaultLaptopWindowValidator(
            new List<IDefaultLaptopWindowRule>
            {
                new DefaultLaptopWindowGradeLengthRule(32),
                new DefaultLaptopWindowAllowedGradeRule(new List<string> { "tiered" }),
                new DefaultLaptopWindowRequiredGradeRule("default laptop window needs this field"),
                new DefaultLaptopWindowLabelLengthRule(86400),
                new DefaultLaptopWindowRequiredLabelRule("default laptop window needs an entry"),
                new DefaultLaptopWindowCodeLengthRule(16),
            });
    }
}
