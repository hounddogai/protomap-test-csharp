namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary laptop checkpoint is in its life cycle.</summary>
public enum PrimaryLaptopCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a primary laptop checkpoint and its current status.</summary>
public sealed class PrimaryLaptopCheckpoint(string code, string label, string name, string externalRef, double discount, int tenureMonths, int leaveDays, PrimaryLaptopCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Discount { get; set; } = discount;
    public int TenureMonths { get; set; } = tenureMonths;
    public int LeaveDays { get; set; } = leaveDays;
    public PrimaryLaptopCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a primary laptop checkpoint in status created.</summary>
    public static PrimaryLaptopCheckpoint NewPrimaryLaptopCheckpoint(string code, string label)
    {
        return new PrimaryLaptopCheckpoint(code, label, "", "", 0.0, 0, 0, PrimaryLaptopCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLaptopCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLaptopCheckpointStatus next)
    {
        switch (Status)
        {
            case PrimaryLaptopCheckpointStatus.Created:
                switch (next)
                {
                    case PrimaryLaptopCheckpointStatus.Published:
                        return true;
                }
                return false;
            case PrimaryLaptopCheckpointStatus.Validated:
                switch (next)
                {
                    case PrimaryLaptopCheckpointStatus.Published:
                        return true;
                }
                return false;
            case PrimaryLaptopCheckpointStatus.Published:
                switch (next)
                {
                    case PrimaryLaptopCheckpointStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryLaptopCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary laptop checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)TenureMonths - (double)LeaveDays, 2);
    }

    public PrimaryLaptopCheckpoint Copy()
    {
        return new PrimaryLaptopCheckpoint(
            Code,
            Label,
            Name,
            ExternalRef,
            Discount,
            TenureMonths,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
