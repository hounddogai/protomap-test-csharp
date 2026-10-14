namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual license seat slot must pass. Check returns its problems, or none.</summary>
public interface IManualLicenseSeatSlotRule
{
    List<string> Check(ManualLicenseSeatSlot item);
}

public sealed record ManualLicenseSeatSlotDurationMinutesRangeRule(int Min, int Max) : IManualLicenseSeatSlotRule
{
    public List<string> Check(ManualLicenseSeatSlot item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLicenseSeatSlotNameLengthRule(int MaxLength) : IManualLicenseSeatSlotRule
{
    public List<string> Check(ManualLicenseSeatSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLicenseSeatSlotCodeLengthRule(int MaxLength) : IManualLicenseSeatSlotRule
{
    public List<string> Check(ManualLicenseSeatSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLicenseSeatSlotAllowedCodeRule(List<string> Allowed) : IManualLicenseSeatSlotRule
{
    public List<string> Check(ManualLicenseSeatSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ManualLicenseSeatSlotRequiredCodeRule(string Message) : IManualLicenseSeatSlotRule
{
    public List<string> Check(ManualLicenseSeatSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual license seat slot.</summary>
public sealed record ManualLicenseSeatSlotValidator(List<IManualLicenseSeatSlotRule> Rules)
{
    public List<string> Validate(ManualLicenseSeatSlot item)
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

    public void RequireValid(ManualLicenseSeatSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualLicenseSeatSlot> items)
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

/// <summary>Functions of manual license seat slot rules.</summary>
public static class ManualLicenseSeatSlotRules
{
    public static ManualLicenseSeatSlotValidator DefaultManualLicenseSeatSlotValidator()
    {
        return new ManualLicenseSeatSlotValidator(
            new List<IManualLicenseSeatSlotRule>
            {
                new ManualLicenseSeatSlotDurationMinutesRangeRule(32, 3600),
                new ManualLicenseSeatSlotNameLengthRule(32),
                new ManualLicenseSeatSlotCodeLengthRule(16),
                new ManualLicenseSeatSlotAllowedCodeRule(new List<string> { "scheduled" }),
                new ManualLicenseSeatSlotRequiredCodeRule("manual license seat slot needs this field"),
            });
    }
}
