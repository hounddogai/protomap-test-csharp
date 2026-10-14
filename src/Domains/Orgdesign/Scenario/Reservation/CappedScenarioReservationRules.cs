namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped scenario reservation must pass. Check returns its problems, or none.</summary>
public interface ICappedScenarioReservationRule
{
    List<string> Check(CappedScenarioReservation item);
}

public sealed record CappedScenarioReservationCodePrefixRule(string Prefix) : ICappedScenarioReservationRule
{
    public List<string> Check(CappedScenarioReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedScenarioReservationPriorityRangeRule(int Min, int Max) : ICappedScenarioReservationRule
{
    public List<string> Check(CappedScenarioReservation item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedScenarioReservationAllowedLabelRule(List<string> Allowed) : ICappedScenarioReservationRule
{
    public List<string> Check(CappedScenarioReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped scenario reservation.</summary>
public sealed record CappedScenarioReservationValidator(List<ICappedScenarioReservationRule> Rules)
{
    public List<string> Validate(CappedScenarioReservation item)
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

    public void RequireValid(CappedScenarioReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedScenarioReservation> items)
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

/// <summary>Functions of capped scenario reservation rules.</summary>
public static class CappedScenarioReservationRules
{
    public static CappedScenarioReservationValidator DefaultCappedScenarioReservationValidator()
    {
        return new CappedScenarioReservationValidator(
            new List<ICappedScenarioReservationRule>
            {
                new CappedScenarioReservationCodePrefixRule("X"),
                new CappedScenarioReservationPriorityRangeRule(64, 120),
                new CappedScenarioReservationAllowedLabelRule(new List<string> { "fallback" }),
            });
    }
}
