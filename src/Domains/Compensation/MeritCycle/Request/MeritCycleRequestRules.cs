namespace Acme.Workforce.Compensation.MeritCycle.Request;

/// <summary>A rule a merit cycle request must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleRequestRule
{
    List<string> Check(MeritCycleRequest item);
}

public sealed record MeritCycleRequestCodePrefixRule(string Prefix) : IMeritCycleRequestRule
{
    public List<string> Check(MeritCycleRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRequestRequiredManagerIdRule(string Message) : IMeritCycleRequestRule
{
    public List<string> Check(MeritCycleRequest item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRequestRatingRangeRule(double Min, double Max) : IMeritCycleRequestRule
{
    public List<string> Check(MeritCycleRequest item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRequestRequiredNameRule(string Message) : IMeritCycleRequestRule
{
    public List<string> Check(MeritCycleRequest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRequestMarginRangeRule(double Min, double Max) : IMeritCycleRequestRule
{
    public List<string> Check(MeritCycleRequest item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a merit cycle request.</summary>
public sealed record MeritCycleRequestValidator(List<IMeritCycleRequestRule> Rules)
{
    public List<string> Validate(MeritCycleRequest item)
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

    public void RequireValid(MeritCycleRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleRequest> items)
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

/// <summary>Functions of merit cycle request rules.</summary>
public static class MeritCycleRequestRules
{
    public static MeritCycleRequestValidator DefaultMeritCycleRequestValidator()
    {
        return new MeritCycleRequestValidator(
            new List<IMeritCycleRequestRule>
            {
                new MeritCycleRequestCodePrefixRule("ACM"),
                new MeritCycleRequestRequiredManagerIdRule("merit cycle request needs a value"),
                new MeritCycleRequestRatingRangeRule(4.6, 287.3),
                new MeritCycleRequestRequiredNameRule("merit cycle request needs a value"),
                new MeritCycleRequestMarginRangeRule(9.6, 3760.8),
            });
    }
}
