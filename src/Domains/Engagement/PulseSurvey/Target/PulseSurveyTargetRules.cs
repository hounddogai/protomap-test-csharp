namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey target must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyTargetRule
{
    List<string> Check(PulseSurveyTarget item);
}

public sealed record PulseSurveyTargetAllowedEmployeeIdRule(List<string> Allowed) : IPulseSurveyTargetRule
{
    public List<string> Check(PulseSurveyTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyTargetRequiredEmployeeIdRule(string Message) : IPulseSurveyTargetRule
{
    public List<string> Check(PulseSurveyTarget item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyTargetCostRangeRule(double Min, double Max) : IPulseSurveyTargetRule
{
    public List<string> Check(PulseSurveyTarget item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey target.</summary>
public sealed record PulseSurveyTargetValidator(List<IPulseSurveyTargetRule> Rules)
{
    public List<string> Validate(PulseSurveyTarget item)
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

    public void RequireValid(PulseSurveyTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyTarget> items)
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

/// <summary>Functions of pulse survey target rules.</summary>
public static class PulseSurveyTargetRules
{
    public static PulseSurveyTargetValidator DefaultPulseSurveyTargetValidator()
    {
        return new PulseSurveyTargetValidator(
            new List<IPulseSurveyTargetRule>
            {
                new PulseSurveyTargetAllowedEmployeeIdRule(new List<string>()),
                new PulseSurveyTargetRequiredEmployeeIdRule("pulse survey target needs a value"),
                new PulseSurveyTargetCostRangeRule(7.3, 1608.5),
            });
    }
}
