namespace Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>A rule a garnishment profile must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentProfileRule
{
    List<string> Check(GarnishmentProfile item);
}

public sealed record GarnishmentProfileCodePrefixRule(string Prefix) : IGarnishmentProfileRule
{
    public List<string> Check(GarnishmentProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentProfileEndsAtRangeRule(int Min, int Max) : IGarnishmentProfileRule
{
    public List<string> Check(GarnishmentProfile item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentProfileRequiredCodeRule(string Message) : IGarnishmentProfileRule
{
    public List<string> Check(GarnishmentProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment profile.</summary>
public sealed record GarnishmentProfileValidator(List<IGarnishmentProfileRule> Rules)
{
    public List<string> Validate(GarnishmentProfile item)
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

    public void RequireValid(GarnishmentProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentProfile> items)
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

/// <summary>Functions of garnishment profile rules.</summary>
public static class GarnishmentProfileRules
{
    public static GarnishmentProfileValidator DefaultGarnishmentProfileValidator()
    {
        return new GarnishmentProfileValidator(
            new List<IGarnishmentProfileRule>
            {
                new GarnishmentProfileCodePrefixRule("Q"),
                new GarnishmentProfileEndsAtRangeRule(80, 255),
                new GarnishmentProfileRequiredCodeRule("garnishment profile needs an entry"),
            });
    }
}
