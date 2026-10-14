namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey reservation must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyReservationRule
{
    List<string> Check(PulseSurveyReservation item);
}

public sealed record PulseSurveyReservationCeilingRangeRule(double Min, double Max) : IPulseSurveyReservationRule
{
    public List<string> Check(PulseSurveyReservation item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyReservationAllowedCodeRule(List<string> Allowed) : IPulseSurveyReservationRule
{
    public List<string> Check(PulseSurveyReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyReservationRevisionRangeRule(int Min, int Max) : IPulseSurveyReservationRule
{
    public List<string> Check(PulseSurveyReservation item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyReservationCodeLengthRule(int MaxLength) : IPulseSurveyReservationRule
{
    public List<string> Check(PulseSurveyReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyReservationCodePrefixRule(string Prefix) : IPulseSurveyReservationRule
{
    public List<string> Check(PulseSurveyReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyReservationFteRangeRule(double Min, double Max) : IPulseSurveyReservationRule
{
    public List<string> Check(PulseSurveyReservation item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey reservation.</summary>
public sealed record PulseSurveyReservationValidator(List<IPulseSurveyReservationRule> Rules)
{
    public List<string> Validate(PulseSurveyReservation item)
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

    public void RequireValid(PulseSurveyReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyReservation> items)
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

/// <summary>Functions of pulse survey reservation rules.</summary>
public static class PulseSurveyReservationRules
{
    public static PulseSurveyReservationValidator DefaultPulseSurveyReservationValidator()
    {
        return new PulseSurveyReservationValidator(
            new List<IPulseSurveyReservationRule>
            {
                new PulseSurveyReservationCeilingRangeRule(4.0, 368.6),
                new PulseSurveyReservationAllowedCodeRule(new List<string> { "tiered" }),
                new PulseSurveyReservationRevisionRangeRule(16, 32),
                new PulseSurveyReservationCodeLengthRule(16),
                new PulseSurveyReservationCodePrefixRule("OPS"),
                new PulseSurveyReservationFteRangeRule(5.3, 4917.0),
            });
    }
}
