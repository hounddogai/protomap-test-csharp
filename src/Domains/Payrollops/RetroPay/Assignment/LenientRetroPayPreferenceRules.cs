namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>A rule a lenient retro pay preference must pass. Check returns its problems, or none.</summary>
public interface ILenientRetroPayPreferenceRule
{
    List<string> Check(LenientRetroPayPreference item);
}

public sealed record LenientRetroPayPreferenceRequiredPhoneRule(string Message) : ILenientRetroPayPreferenceRule
{
    public List<string> Check(LenientRetroPayPreference item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPayPreferenceRequiredCodeRule(string Message) : ILenientRetroPayPreferenceRule
{
    public List<string> Check(LenientRetroPayPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPayPreferenceSequenceRangeRule(int Min, int Max) : ILenientRetroPayPreferenceRule
{
    public List<string> Check(LenientRetroPayPreference item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPayPreferenceRequiredLocationRule(string Message) : ILenientRetroPayPreferenceRule
{
    public List<string> Check(LenientRetroPayPreference item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPayPreferenceTitleLengthRule(int MaxLength) : ILenientRetroPayPreferenceRule
{
    public List<string> Check(LenientRetroPayPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPayPreferenceCodeLengthRule(int MaxLength) : ILenientRetroPayPreferenceRule
{
    public List<string> Check(LenientRetroPayPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient retro pay preference.</summary>
public sealed record LenientRetroPayPreferenceValidator(List<ILenientRetroPayPreferenceRule> Rules)
{
    public List<string> Validate(LenientRetroPayPreference item)
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

    public void RequireValid(LenientRetroPayPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRetroPayPreference> items)
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

/// <summary>Functions of lenient retro pay preference rules.</summary>
public static class LenientRetroPayPreferenceRules
{
    public static LenientRetroPayPreferenceValidator DefaultLenientRetroPayPreferenceValidator()
    {
        return new LenientRetroPayPreferenceValidator(
            new List<ILenientRetroPayPreferenceRule>
            {
                new LenientRetroPayPreferenceRequiredPhoneRule("lenient retro pay preference needs a value"),
                new LenientRetroPayPreferenceRequiredCodeRule("lenient retro pay preference needs this field"),
                new LenientRetroPayPreferenceSequenceRangeRule(32, 1000),
                new LenientRetroPayPreferenceRequiredLocationRule("lenient retro pay preference needs a value"),
                new LenientRetroPayPreferenceTitleLengthRule(16),
                new LenientRetroPayPreferenceCodeLengthRule(120),
            });
    }
}
