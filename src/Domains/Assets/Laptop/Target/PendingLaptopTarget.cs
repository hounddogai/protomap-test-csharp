namespace Acme.Workforce.Assets.Laptop.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending laptop target is in its life cycle.</summary>
public enum PendingLaptopTargetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending laptop target. Its code is unique within a tenant.</summary>
public sealed class PendingLaptopTarget(string code, string label, int batchSize, bool visible, bool archived, string contactEmail, int expiresAt, string reviewer, double ceiling, string phone, PendingLaptopTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public bool Visible { get; set; } = visible;
    public bool Archived { get; set; } = archived;
    public string ContactEmail { get; set; } = contactEmail;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Reviewer { get; set; } = reviewer;
    public double Ceiling { get; set; } = ceiling;
    public string Phone { get; set; } = phone;
    public PendingLaptopTargetStatus Status { get; set; } = status;

    /// <summary>Starts a pending laptop target in status draft.</summary>
    public static PendingLaptopTarget NewPendingLaptopTarget(string code, string label)
    {
        return new PendingLaptopTarget(
            code,
            label,
            0,
            false,
            false,
            "",
            0,
            "",
            0.0,
            "",
            PendingLaptopTargetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingLaptopTargetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingLaptopTargetStatus next)
    {
        switch (Status)
        {
            case PendingLaptopTargetStatus.Draft:
                return next == PendingLaptopTargetStatus.Pending;
            case PendingLaptopTargetStatus.Pending:
                return next == PendingLaptopTargetStatus.Active || next == PendingLaptopTargetStatus.Closed;
            case PendingLaptopTargetStatus.Active:
                return next == PendingLaptopTargetStatus.Suspended;
            case PendingLaptopTargetStatus.Suspended:
                switch (next)
                {
                    case PendingLaptopTargetStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingLaptopTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending laptop target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ceiling * (double)ExpiresAt, 2);
    }

    public PendingLaptopTarget Copy()
    {
        return new PendingLaptopTarget(
            Code,
            Label,
            BatchSize,
            Visible,
            Archived,
            ContactEmail,
            ExpiresAt,
            Reviewer,
            Ceiling,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
