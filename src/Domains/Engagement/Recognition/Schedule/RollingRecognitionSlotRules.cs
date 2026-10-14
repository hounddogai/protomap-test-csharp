namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling recognition slot must pass. Check returns its problems, or none.</summary>
public interface IRollingRecognitionSlotRule
{
    List<string> Check(RollingRecognitionSlot item);
}

public sealed record RollingRecognitionSlotRequiredPhoneRule(string Message) : IRollingRecognitionSlotRule
{
    public List<string> Check(RollingRecognitionSlot item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRecognitionSlotCurrencyLengthRule(int MaxLength) : IRollingRecognitionSlotRule
{
    public List<string> Check(RollingRecognitionSlot item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRecognitionSlotAllowedPhoneRule(List<string> Allowed) : IRollingRecognitionSlotRule
{
    public List<string> Check(RollingRecognitionSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling recognition slot.</summary>
public sealed record RollingRecognitionSlotValidator(List<IRollingRecognitionSlotRule> Rules)
{
    public List<string> Validate(RollingRecognitionSlot item)
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

    public void RequireValid(RollingRecognitionSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRecognitionSlot> items)
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

/// <summary>Functions of rolling recognition slot rules.</summary>
public static class RollingRecognitionSlotRules
{
    public static RollingRecognitionSlotValidator DefaultRollingRecognitionSlotValidator()
    {
        return new RollingRecognitionSlotValidator(
            new List<IRollingRecognitionSlotRule>
            {
                new RollingRecognitionSlotRequiredPhoneRule("rolling recognition slot needs a value"),
                new RollingRecognitionSlotCurrencyLengthRule(3600),
                new RollingRecognitionSlotAllowedPhoneRule(new List<string> { "strict", "rolling" }),
            });
    }
}
