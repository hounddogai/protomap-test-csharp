namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay batch must pass. Check returns its problems, or none.</summary>
public interface IRetroPayBatchRule
{
    List<string> Check(RetroPayBatch item);
}

public sealed record RetroPayBatchCodePrefixRule(string Prefix) : IRetroPayBatchRule
{
    public List<string> Check(RetroPayBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayBatchAllowedCodeRule(List<string> Allowed) : IRetroPayBatchRule
{
    public List<string> Check(RetroPayBatch item)
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

public sealed record RetroPayBatchRequiredCodeRule(string Message) : IRetroPayBatchRule
{
    public List<string> Check(RetroPayBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayBatchThresholdRangeRule(double Min, double Max) : IRetroPayBatchRule
{
    public List<string> Check(RetroPayBatch item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayBatchScoreRangeRule(double Min, double Max) : IRetroPayBatchRule
{
    public List<string> Check(RetroPayBatch item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayBatchRequiredReviewerRule(string Message) : IRetroPayBatchRule
{
    public List<string> Check(RetroPayBatch item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay batch.</summary>
public sealed record RetroPayBatchValidator(List<IRetroPayBatchRule> Rules)
{
    public List<string> Validate(RetroPayBatch item)
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

    public void RequireValid(RetroPayBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayBatch> items)
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

/// <summary>Functions of retro pay batch rules.</summary>
public static class RetroPayBatchRules
{
    public static RetroPayBatchValidator DefaultRetroPayBatchValidator()
    {
        return new RetroPayBatchValidator(
            new List<IRetroPayBatchRule>
            {
                new RetroPayBatchCodePrefixRule("REF"),
                new RetroPayBatchAllowedCodeRule(new List<string>()),
                new RetroPayBatchRequiredCodeRule("retro pay batch needs this field"),
                new RetroPayBatchThresholdRangeRule(6.9, 78.4),
                new RetroPayBatchScoreRangeRule(3.0, 4007.0),
                new RetroPayBatchRequiredReviewerRule("retro pay batch needs an entry"),
            });
    }
}
