namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path notice must pass. Check returns its problems, or none.</summary>
public interface ICareerPathNoticeRule
{
    List<string> Check(CareerPathNotice item);
}

public sealed record CareerPathNoticeRequiredCodeRule(string Message) : ICareerPathNoticeRule
{
    public List<string> Check(CareerPathNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathNoticeRatioRangeRule(double Min, double Max) : ICareerPathNoticeRule
{
    public List<string> Check(CareerPathNotice item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathNoticeRequiredLabelRule(string Message) : ICareerPathNoticeRule
{
    public List<string> Check(CareerPathNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathNoticeAllowedCodeRule(List<string> Allowed) : ICareerPathNoticeRule
{
    public List<string> Check(CareerPathNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path notice.</summary>
public sealed record CareerPathNoticeValidator(List<ICareerPathNoticeRule> Rules)
{
    public List<string> Validate(CareerPathNotice item)
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

    public void RequireValid(CareerPathNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathNotice> items)
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

/// <summary>Functions of career path notice rules.</summary>
public static class CareerPathNoticeRules
{
    public static CareerPathNoticeValidator DefaultCareerPathNoticeValidator()
    {
        return new CareerPathNoticeValidator(
            new List<ICareerPathNoticeRule>
            {
                new CareerPathNoticeRequiredCodeRule("career path notice needs an entry"),
                new CareerPathNoticeRatioRangeRule(8.5, 1753.6),
                new CareerPathNoticeRequiredLabelRule("career path notice needs an entry"),
                new CareerPathNoticeAllowedCodeRule(new List<string> { "primary", "rolling", "tiered" }),
            });
    }
}
