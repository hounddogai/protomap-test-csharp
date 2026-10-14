namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction request is in its life cycle.</summary>
public enum DeductionRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One deduction request, with what the planners need to act on it.</summary>
public sealed class DeductionRequest(string code, string name, string country, double amount, double minimum, Dictionary<string, string> attributes, string employeeId, int version, string team, string region, string unit, DeductionRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Amount { get; set; } = amount;
    public double Minimum { get; set; } = minimum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string EmployeeId { get; set; } = employeeId;
    public int Version { get; set; } = version;
    public string Team { get; set; } = team;
    public string Region { get; set; } = region;
    public string Unit { get; set; } = unit;
    public DeductionRequestStatus Status { get; set; } = status;

    /// <summary>Starts a deduction request in status new.</summary>
    public static DeductionRequest NewDeductionRequest(string code, string name)
    {
        return new DeductionRequest(
            code,
            name,
            "",
            0.0,
            0.0,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            "",
            "",
            DeductionRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionRequestStatus next)
    {
        switch (Status)
        {
            case DeductionRequestStatus.New:
                switch (next)
                {
                    case DeductionRequestStatus.Queued:
                    case DeductionRequestStatus.Running:
                        return true;
                }
                return false;
            case DeductionRequestStatus.Queued:
                switch (next)
                {
                    case DeductionRequestStatus.Failed:
                        return true;
                }
                return false;
            case DeductionRequestStatus.Running:
                return next == DeductionRequestStatus.Done;
            case DeductionRequestStatus.Done:
                return next == DeductionRequestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DeductionRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedLoad()
    {
        return Math.Round(Minimum + Amount, 2);
    }

    public DeductionRequest Copy()
    {
        return new DeductionRequest(
            Code,
            Name,
            Country,
            Amount,
            Minimum,
            Attributes,
            EmployeeId,
            Version,
            Team,
            Region,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
