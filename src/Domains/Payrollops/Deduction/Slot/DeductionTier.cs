namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction tier is in its life cycle.</summary>
public enum DeductionTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A deduction tier. Its code is unique within a tenant.</summary>
public sealed class DeductionTier(string code, string title, double weight, int version, double rate, string phone, int retries, double floorValue, int sequence, bool visible, string checksum, DeductionTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public int Version { get; set; } = version;
    public double Rate { get; set; } = rate;
    public string Phone { get; set; } = phone;
    public int Retries { get; set; } = retries;
    public double FloorValue { get; set; } = floorValue;
    public int Sequence { get; set; } = sequence;
    public bool Visible { get; set; } = visible;
    public string Checksum { get; set; } = checksum;
    public DeductionTierStatus Status { get; set; } = status;

    /// <summary>Starts a deduction tier in status proposed.</summary>
    public static DeductionTier NewDeductionTier(string code, string title)
    {
        return new DeductionTier(code, title, 0.0, 0, 0.0, "", 0, 0.0, 0, false, "", DeductionTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionTierStatus next)
    {
        switch (Status)
        {
            case DeductionTierStatus.Proposed:
                switch (next)
                {
                    case DeductionTierStatus.Rejected:
                        return true;
                }
                return false;
            case DeductionTierStatus.Approved:
                return next == DeductionTierStatus.Rejected || next == DeductionTierStatus.Withdrawn;
            case DeductionTierStatus.Rejected:
                switch (next)
                {
                    case DeductionTierStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Rate * (double)Retries, 2);
    }

    public DeductionTier Copy()
    {
        return new DeductionTier(
            Code,
            Title,
            Weight,
            Version,
            Rate,
            Phone,
            Retries,
            FloorValue,
            Sequence,
            Visible,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
