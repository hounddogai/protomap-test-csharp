namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation schedule is in its life cycle.</summary>
public enum AttestationScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A attestation schedule as the attestation team keeps it.</summary>
public sealed class AttestationSchedule(string code, string name, bool archived, string currency, string unit, string jobCode, string externalRef, double rate, int attempts, string badge, string channel, AttestationScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Currency { get; set; } = currency;
    public string Unit { get; set; } = unit;
    public string JobCode { get; set; } = jobCode;
    public string ExternalRef { get; set; } = externalRef;
    public double Rate { get; set; } = rate;
    public int Attempts { get; set; } = attempts;
    public string Badge { get; set; } = badge;
    public string Channel { get; set; } = channel;
    public AttestationScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a attestation schedule in status created.</summary>
    public static AttestationSchedule NewAttestationSchedule(string code, string name)
    {
        return new AttestationSchedule(
            code,
            name,
            false,
            "",
            "",
            "",
            "",
            0.0,
            0,
            "",
            "",
            AttestationScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationScheduleStatus next)
    {
        switch (Status)
        {
            case AttestationScheduleStatus.Created:
                return next == AttestationScheduleStatus.Validated || next == AttestationScheduleStatus.Retired;
            case AttestationScheduleStatus.Validated:
                switch (next)
                {
                    case AttestationScheduleStatus.Published:
                        return true;
                }
                return false;
            case AttestationScheduleStatus.Published:
                switch (next)
                {
                    case AttestationScheduleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"attestation schedule"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Attempts + Rate, 2);
    }

    public AttestationSchedule Copy()
    {
        return new AttestationSchedule(
            Code,
            Name,
            Archived,
            Currency,
            Unit,
            JobCode,
            ExternalRef,
            Rate,
            Attempts,
            Badge,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
