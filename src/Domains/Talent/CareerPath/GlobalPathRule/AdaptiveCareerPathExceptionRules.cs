namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive career path exception must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCareerPathExceptionRule
{
    List<string> Check(AdaptiveCareerPathException item);
}

public sealed record AdaptiveCareerPathExceptionTenureMonthsRangeRule(int Min, int Max) : IAdaptiveCareerPathExceptionRule
{
    public List<string> Check(AdaptiveCareerPathException item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathExceptionRequiredCodeRule(string Message) : IAdaptiveCareerPathExceptionRule
{
    public List<string> Check(AdaptiveCareerPathException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathExceptionAllowedCodeRule(List<string> Allowed) : IAdaptiveCareerPathExceptionRule
{
    public List<string> Check(AdaptiveCareerPathException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive career path exception.</summary>
public sealed record AdaptiveCareerPathExceptionValidator(List<IAdaptiveCareerPathExceptionRule> Rules)
{
    public List<string> Validate(AdaptiveCareerPathException item)
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

    public void RequireValid(AdaptiveCareerPathException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCareerPathException> items)
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

/// <summary>Functions of adaptive career path exception rules.</summary>
public static class AdaptiveCareerPathExceptionRules
{
    public static AdaptiveCareerPathExceptionValidator DefaultAdaptiveCareerPathExceptionValidator()
    {
        return new AdaptiveCareerPathExceptionValidator(
            new List<IAdaptiveCareerPathExceptionRule>
            {
                new AdaptiveCareerPathExceptionTenureMonthsRangeRule(80, 120),
                new AdaptiveCareerPathExceptionRequiredCodeRule("adaptive career path exception needs a value"),
                new AdaptiveCareerPathExceptionAllowedCodeRule(new List<string>()),
            });
    }
}
