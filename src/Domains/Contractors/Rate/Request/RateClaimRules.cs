namespace Acme.Workforce.Contractors.Rate.Request;

/// <summary>A rule a rate claim must pass. Check returns its problems, or none.</summary>
public interface IRateClaimRule
{
    List<string> Check(RateClaim item);
}

public sealed record RateClaimRequiredNameRule(string Message) : IRateClaimRule
{
    public List<string> Check(RateClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateClaimRequiredCodeRule(string Message) : IRateClaimRule
{
    public List<string> Check(RateClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateClaimCodeLengthRule(int MaxLength) : IRateClaimRule
{
    public List<string> Check(RateClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateClaimRequiredJobCodeRule(string Message) : IRateClaimRule
{
    public List<string> Check(RateClaim item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate claim.</summary>
public sealed record RateClaimValidator(List<IRateClaimRule> Rules)
{
    public List<string> Validate(RateClaim item)
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

    public void RequireValid(RateClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateClaim> items)
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

/// <summary>Functions of rate claim rules.</summary>
public static class RateClaimRules
{
    public static RateClaimValidator DefaultRateClaimValidator()
    {
        return new RateClaimValidator(
            new List<IRateClaimRule>
            {
                new RateClaimRequiredNameRule("rate claim needs this field"),
                new RateClaimRequiredCodeRule("rate claim needs this field"),
                new RateClaimCodeLengthRule(64),
                new RateClaimRequiredJobCodeRule("rate claim needs this field"),
            });
    }
}
