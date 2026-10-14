namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip checkpoint is in its life cycle.</summary>
public enum PayslipCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a payslip checkpoint and its current status.</summary>
public sealed class PayslipCheckpoint(string code, string name, bool visible, int createdAt, string skill, double rate, double weight, double hours, string notes, int startsAt, PayslipCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public int CreatedAt { get; set; } = createdAt;
    public string Skill { get; set; } = skill;
    public double Rate { get; set; } = rate;
    public double Weight { get; set; } = weight;
    public double Hours { get; set; } = hours;
    public string Notes { get; set; } = notes;
    public int StartsAt { get; set; } = startsAt;
    public PayslipCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a payslip checkpoint in status created.</summary>
    public static PayslipCheckpoint NewPayslipCheckpoint(string code, string name)
    {
        return new PayslipCheckpoint(
            code,
            name,
            false,
            0,
            "",
            0.0,
            0.0,
            0.0,
            "",
            0,
            PayslipCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipCheckpointStatus next)
    {
        switch (Status)
        {
            case PayslipCheckpointStatus.Created:
                switch (next)
                {
                    case PayslipCheckpointStatus.Published:
                    case PayslipCheckpointStatus.Retired:
                        return true;
                }
                return false;
            case PayslipCheckpointStatus.Validated:
                switch (next)
                {
                    case PayslipCheckpointStatus.Published:
                        return true;
                }
                return false;
            case PayslipCheckpointStatus.Published:
                switch (next)
                {
                    case PayslipCheckpointStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Weight * (double)StartsAt, 2);
    }

    public PayslipCheckpoint Copy()
    {
        return new PayslipCheckpoint(
            Code,
            Name,
            Visible,
            CreatedAt,
            Skill,
            Rate,
            Weight,
            Hours,
            Notes,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
