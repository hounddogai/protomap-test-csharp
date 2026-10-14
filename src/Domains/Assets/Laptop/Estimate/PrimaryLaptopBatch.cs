namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary laptop batch is in its life cycle.</summary>
public enum PrimaryLaptopBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary laptop batch. Its code is unique within a tenant.</summary>
public sealed class PrimaryLaptopBatch(string code, string label, string unit, int expiresAt, string title, double ceiling, string category, bool active, string source, PrimaryLaptopBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public string Source { get; set; } = source;
    public PrimaryLaptopBatchStatus Status { get; set; } = status;

    /// <summary>Starts a primary laptop batch in status requested.</summary>
    public static PrimaryLaptopBatch NewPrimaryLaptopBatch(string code, string label)
    {
        return new PrimaryLaptopBatch(
            code,
            label,
            "",
            0,
            "",
            0.0,
            "",
            false,
            "",
            PrimaryLaptopBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLaptopBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLaptopBatchStatus next)
    {
        switch (Status)
        {
            case PrimaryLaptopBatchStatus.Requested:
                switch (next)
                {
                    case PrimaryLaptopBatchStatus.Revoked:
                        return true;
                }
                return false;
            case PrimaryLaptopBatchStatus.Granted:
                return next == PrimaryLaptopBatchStatus.Revoked || next == PrimaryLaptopBatchStatus.Expired;
            case PrimaryLaptopBatchStatus.Revoked:
                switch (next)
                {
                    case PrimaryLaptopBatchStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryLaptopBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary laptop batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Ceiling * (double)ExpiresAt, 2);
    }

    public PrimaryLaptopBatch Copy()
    {
        return new PrimaryLaptopBatch(
            Code,
            Label,
            Unit,
            ExpiresAt,
            Title,
            Ceiling,
            Category,
            Active,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
