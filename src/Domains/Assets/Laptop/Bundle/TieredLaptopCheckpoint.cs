namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered laptop checkpoint is in its life cycle.</summary>
public enum TieredLaptopCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One tiered laptop checkpoint, with what the planners need to act on it.</summary>
public sealed class TieredLaptopCheckpoint(string code, string label, string jobCode, bool approved, int capacity, double ceiling, string badge, string skill, TieredLaptopCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public bool Approved { get; set; } = approved;
    public int Capacity { get; set; } = capacity;
    public double Ceiling { get; set; } = ceiling;
    public string Badge { get; set; } = badge;
    public string Skill { get; set; } = skill;
    public TieredLaptopCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a tiered laptop checkpoint in status created.</summary>
    public static TieredLaptopCheckpoint NewTieredLaptopCheckpoint(string code, string label)
    {
        return new TieredLaptopCheckpoint(
            code,
            label,
            "",
            false,
            0,
            0.0,
            "",
            "",
            TieredLaptopCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLaptopCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLaptopCheckpointStatus next)
    {
        switch (Status)
        {
            case TieredLaptopCheckpointStatus.Created:
                switch (next)
                {
                    case TieredLaptopCheckpointStatus.Validated:
                    case TieredLaptopCheckpointStatus.Published:
                        return true;
                }
                return false;
            case TieredLaptopCheckpointStatus.Validated:
                return next == TieredLaptopCheckpointStatus.Published || next == TieredLaptopCheckpointStatus.Retired;
            case TieredLaptopCheckpointStatus.Published:
                return next == TieredLaptopCheckpointStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredLaptopCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered laptop checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Capacity + Ceiling, 2);
    }

    public TieredLaptopCheckpoint Copy()
    {
        return new TieredLaptopCheckpoint(Code, Label, JobCode, Approved, Capacity, Ceiling, Badge, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
