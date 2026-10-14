namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario reservation must pass. Check returns its problems, or none.</summary>
public interface IScenarioReservationRule
{
    List<string> Check(ScenarioReservation item);
}

public sealed record ScenarioReservationAllowedPhoneRule(List<string> Allowed) : IScenarioReservationRule
{
    public List<string> Check(ScenarioReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioReservationCodePrefixRule(string Prefix) : IScenarioReservationRule
{
    public List<string> Check(ScenarioReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioReservationRequiredGradeRule(string Message) : IScenarioReservationRule
{
    public List<string> Check(ScenarioReservation item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioReservationRatioRangeRule(double Min, double Max) : IScenarioReservationRule
{
    public List<string> Check(ScenarioReservation item)
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

public sealed record ScenarioReservationGradeLengthRule(int MaxLength) : IScenarioReservationRule
{
    public List<string> Check(ScenarioReservation item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioReservationLeaveDaysRangeRule(int Min, int Max) : IScenarioReservationRule
{
    public List<string> Check(ScenarioReservation item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario reservation.</summary>
public sealed record ScenarioReservationValidator(List<IScenarioReservationRule> Rules)
{
    public List<string> Validate(ScenarioReservation item)
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

    public void RequireValid(ScenarioReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioReservation> items)
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

/// <summary>Functions of scenario reservation rules.</summary>
public static class ScenarioReservationRules
{
    public static ScenarioReservationValidator DefaultScenarioReservationValidator()
    {
        return new ScenarioReservationValidator(
            new List<IScenarioReservationRule>
            {
                new ScenarioReservationAllowedPhoneRule(new List<string> { "global" }),
                new ScenarioReservationCodePrefixRule("REF"),
                new ScenarioReservationRequiredGradeRule("scenario reservation needs an entry"),
                new ScenarioReservationRatioRangeRule(6.8, 3764.7),
                new ScenarioReservationGradeLengthRule(1000),
                new ScenarioReservationLeaveDaysRangeRule(32, 1000),
            });
    }
}
