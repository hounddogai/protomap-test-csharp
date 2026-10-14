namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday cycle must pass. Check returns its problems, or none.</summary>
public interface IHolidayCycleRule
{
    List<string> Check(HolidayCycle item);
}

public sealed record HolidayCycleTitleLengthRule(int MaxLength) : IHolidayCycleRule
{
    public List<string> Check(HolidayCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayCycleApprovedGuardRule(int Limit) : IHolidayCycleRule
{
    public List<string> Check(HolidayCycle item)
    {
        if (item.Approved && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record HolidayCycleAllowedChecksumRule(List<string> Allowed) : IHolidayCycleRule
{
    public List<string> Check(HolidayCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is too long" };
        }
        return new List<string>();
    }
}

public sealed record HolidayCycleRequiredChecksumRule(string Message) : IHolidayCycleRule
{
    public List<string> Check(HolidayCycle item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayCycleChecksumLengthRule(int MaxLength) : IHolidayCycleRule
{
    public List<string> Check(HolidayCycle item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday cycle.</summary>
public sealed record HolidayCycleValidator(List<IHolidayCycleRule> Rules)
{
    public List<string> Validate(HolidayCycle item)
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

    public void RequireValid(HolidayCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayCycle> items)
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

/// <summary>Functions of holiday cycle rules.</summary>
public static class HolidayCycleRules
{
    public static HolidayCycleValidator DefaultHolidayCycleValidator()
    {
        return new HolidayCycleValidator(
            new List<IHolidayCycleRule>
            {
                new HolidayCycleTitleLengthRule(86400),
                new HolidayCycleApprovedGuardRule(32),
                new HolidayCycleAllowedChecksumRule(new List<string> { "tiered", "fallback", "regional" }),
                new HolidayCycleRequiredChecksumRule("holiday cycle needs an entry"),
                new HolidayCycleChecksumLengthRule(64),
            });
    }
}
