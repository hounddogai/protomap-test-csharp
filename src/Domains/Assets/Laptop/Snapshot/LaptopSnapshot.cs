namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop snapshot is in its life cycle.</summary>
public enum LaptopSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A laptop snapshot as the laptop team keeps it.</summary>
public sealed class LaptopSnapshot(string code, string title, string ownerId, string checksum, int updatedAt, string reviewer, int quantity, bool visible, double budget, LaptopSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Checksum { get; set; } = checksum;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Reviewer { get; set; } = reviewer;
    public int Quantity { get; set; } = quantity;
    public bool Visible { get; set; } = visible;
    public double Budget { get; set; } = budget;
    public LaptopSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a laptop snapshot in status draft.</summary>
    public static LaptopSnapshot NewLaptopSnapshot(string code, string title)
    {
        return new LaptopSnapshot(code, title, "", "", 0, "", 0, false, 0.0, LaptopSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopSnapshotStatus next)
    {
        switch (Status)
        {
            case LaptopSnapshotStatus.Draft:
                switch (next)
                {
                    case LaptopSnapshotStatus.Active:
                        return true;
                }
                return false;
            case LaptopSnapshotStatus.Pending:
                return next == LaptopSnapshotStatus.Suspended;
            case LaptopSnapshotStatus.Active:
                switch (next)
                {
                    case LaptopSnapshotStatus.Suspended:
                    case LaptopSnapshotStatus.Closed:
                        return true;
                }
                return false;
            case LaptopSnapshotStatus.Suspended:
                switch (next)
                {
                    case LaptopSnapshotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)UpdatedAt + (double)Quantity, 2);
    }

    public LaptopSnapshot Copy()
    {
        return new LaptopSnapshot(
            Code,
            Title,
            OwnerId,
            Checksum,
            UpdatedAt,
            Reviewer,
            Quantity,
            Visible,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
