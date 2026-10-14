namespace Acme.Workforce.Payrollops.RetroPay.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay estimate must pass. Check returns its problems, or none.</summary>
public interface IRetroPayEstimateRule
{
    List<string> Check(RetroPayEstimate item);
}

public sealed record RetroPayEstimateRequiredJobCodeRule(string Message) : IRetroPayEstimateRule
{
    public List<string> Check(RetroPayEstimate item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayEstimateActiveGuardRule(double Limit) : IRetroPayEstimateRule
{
    public List<string> Check(RetroPayEstimate item)
    {
        if (item.Active && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayEstimateSummaryLengthRule(int MaxLength) : IRetroPayEstimateRule
{
    public List<string> Check(RetroPayEstimate item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayEstimateTitleLengthRule(int MaxLength) : IRetroPayEstimateRule
{
    public List<string> Check(RetroPayEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayEstimateAllowedCodeRule(List<string> Allowed) : IRetroPayEstimateRule
{
    public List<string> Check(RetroPayEstimate item)
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

/// <summary>Runs every rule of a retro pay estimate.</summary>
public sealed record RetroPayEstimateValidator(List<IRetroPayEstimateRule> Rules)
{
    public List<string> Validate(RetroPayEstimate item)
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

    public void RequireValid(RetroPayEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayEstimate> items)
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

/// <summary>Functions of retro pay estimate rules.</summary>
public static class RetroPayEstimateRules
{
    public static RetroPayEstimateValidator DefaultRetroPayEstimateValidator()
    {
        return new RetroPayEstimateValidator(
            new List<IRetroPayEstimateRule>
            {
                new RetroPayEstimateRequiredJobCodeRule("retro pay estimate needs this field"),
                new RetroPayEstimateActiveGuardRule(0.6),
                new RetroPayEstimateSummaryLengthRule(120),
                new RetroPayEstimateTitleLengthRule(86400),
                new RetroPayEstimateAllowedCodeRule(new List<string>()),
            });
    }
}
