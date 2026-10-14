namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path window must pass. Check returns its problems, or none.</summary>
public interface ICareerPathWindowRule
{
    List<string> Check(CareerPathWindow item);
}

public sealed record CareerPathWindowTeamLengthRule(int MaxLength) : ICareerPathWindowRule
{
    public List<string> Check(CareerPathWindow item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathWindowRequiredTeamRule(string Message) : ICareerPathWindowRule
{
    public List<string> Check(CareerPathWindow item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathWindowAllowedCodeRule(List<string> Allowed) : ICareerPathWindowRule
{
    public List<string> Check(CareerPathWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path window.</summary>
public sealed record CareerPathWindowValidator(List<ICareerPathWindowRule> Rules)
{
    public List<string> Validate(CareerPathWindow item)
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

    public void RequireValid(CareerPathWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathWindow> items)
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

/// <summary>Functions of career path window rules.</summary>
public static class CareerPathWindowRules
{
    public static CareerPathWindowValidator DefaultCareerPathWindowValidator()
    {
        return new CareerPathWindowValidator(
            new List<ICareerPathWindowRule>
            {
                new CareerPathWindowTeamLengthRule(1000),
                new CareerPathWindowRequiredTeamRule("career path window needs an entry"),
                new CareerPathWindowAllowedCodeRule(new List<string> { "primary", "weighted", "seasonal" }),
            });
    }
}
