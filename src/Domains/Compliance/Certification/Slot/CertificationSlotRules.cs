namespace Acme.Workforce.Compliance.Certification.Slot;

/// <summary>A rule a certification slot must pass. Check returns its problems, or none.</summary>
public interface ICertificationSlotRule
{
    List<string> Check(CertificationSlot item);
}

public sealed record CertificationSlotTenureMonthsRangeRule(int Min, int Max) : ICertificationSlotRule
{
    public List<string> Check(CertificationSlot item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CertificationSlotTitleLengthRule(int MaxLength) : ICertificationSlotRule
{
    public List<string> Check(CertificationSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationSlotRequiredEmployeeIdRule(string Message) : ICertificationSlotRule
{
    public List<string> Check(CertificationSlot item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationSlotAttemptsRangeRule(int Min, int Max) : ICertificationSlotRule
{
    public List<string> Check(CertificationSlot item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a certification slot.</summary>
public sealed record CertificationSlotValidator(List<ICertificationSlotRule> Rules)
{
    public List<string> Validate(CertificationSlot item)
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

    public void RequireValid(CertificationSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationSlot> items)
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

/// <summary>Functions of certification slot rules.</summary>
public static class CertificationSlotRules
{
    public static CertificationSlotValidator DefaultCertificationSlotValidator()
    {
        return new CertificationSlotValidator(
            new List<ICertificationSlotRule>
            {
                new CertificationSlotTenureMonthsRangeRule(120, 255),
                new CertificationSlotTitleLengthRule(64),
                new CertificationSlotRequiredEmployeeIdRule("certification slot needs a value"),
                new CertificationSlotAttemptsRangeRule(1000, 86400),
            });
    }
}
