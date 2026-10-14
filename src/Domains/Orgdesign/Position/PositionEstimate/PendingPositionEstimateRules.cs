namespace Acme.Workforce.Orgdesign.Position.PositionEstimate;

/// <summary>A rule a pending position estimate must pass. Check returns its problems, or none.</summary>
public interface IPendingPositionEstimateRule
{
    List<string> Check(PendingPositionEstimate item);
}

public sealed record PendingPositionEstimateTitleLengthRule(int MaxLength) : IPendingPositionEstimateRule
{
    public List<string> Check(PendingPositionEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPositionEstimateRequiredCodeRule(string Message) : IPendingPositionEstimateRule
{
    public List<string> Check(PendingPositionEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPositionEstimateCodeLengthRule(int MaxLength) : IPendingPositionEstimateRule
{
    public List<string> Check(PendingPositionEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending position estimate.</summary>
public sealed record PendingPositionEstimateValidator(List<IPendingPositionEstimateRule> Rules)
{
    public List<string> Validate(PendingPositionEstimate item)
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

    public void RequireValid(PendingPositionEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPositionEstimate> items)
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

/// <summary>Functions of pending position estimate rules.</summary>
public static class PendingPositionEstimateRules
{
    public static PendingPositionEstimateValidator DefaultPendingPositionEstimateValidator()
    {
        return new PendingPositionEstimateValidator(
            new List<IPendingPositionEstimateRule>
            {
                new PendingPositionEstimateTitleLengthRule(80),
                new PendingPositionEstimateRequiredCodeRule("pending position estimate needs an entry"),
                new PendingPositionEstimateCodeLengthRule(64),
            });
    }
}
