namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination window must pass. Check returns its problems, or none.</summary>
public interface INominationWindowRule
{
    List<string> Check(NominationWindow item);
}

public sealed record NominationWindowAllowedCodeRule(List<string> Allowed) : INominationWindowRule
{
    public List<string> Check(NominationWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record NominationWindowCountryLengthRule(int MaxLength) : INominationWindowRule
{
    public List<string> Check(NominationWindow item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationWindowLabelLengthRule(int MaxLength) : INominationWindowRule
{
    public List<string> Check(NominationWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationWindowRatioRangeRule(double Min, double Max) : INominationWindowRule
{
    public List<string> Check(NominationWindow item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationWindowRequiredCountryRule(string Message) : INominationWindowRule
{
    public List<string> Check(NominationWindow item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination window.</summary>
public sealed record NominationWindowValidator(List<INominationWindowRule> Rules)
{
    public List<string> Validate(NominationWindow item)
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

    public void RequireValid(NominationWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationWindow> items)
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

/// <summary>Functions of nomination window rules.</summary>
public static class NominationWindowRules
{
    public static NominationWindowValidator DefaultNominationWindowValidator()
    {
        return new NominationWindowValidator(
            new List<INominationWindowRule>
            {
                new NominationWindowAllowedCodeRule(new List<string> { "default" }),
                new NominationWindowCountryLengthRule(80),
                new NominationWindowLabelLengthRule(1000),
                new NominationWindowRatioRangeRule(5.0, 2796.1),
                new NominationWindowRequiredCountryRule("nomination window needs this field"),
            });
    }
}
