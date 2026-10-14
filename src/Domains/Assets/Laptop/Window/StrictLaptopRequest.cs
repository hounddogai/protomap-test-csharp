namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict laptop request is in its life cycle.</summary>
public enum StrictLaptopRequestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A strict laptop request as the laptop team keeps it.</summary>
public sealed class StrictLaptopRequest(string code, string label, double cost, int retries, string segment, double amount, double fte, StrictLaptopRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public int Retries { get; set; } = retries;
    public string Segment { get; set; } = segment;
    public double Amount { get; set; } = amount;
    public double Fte { get; set; } = fte;
    public StrictLaptopRequestStatus Status { get; set; } = status;

    /// <summary>Starts a strict laptop request in status idle.</summary>
    public static StrictLaptopRequest NewStrictLaptopRequest(string code, string label)
    {
        return new StrictLaptopRequest(code, label, 0.0, 0, "", 0.0, 0.0, StrictLaptopRequestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictLaptopRequestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictLaptopRequestStatus next)
    {
        switch (Status)
        {
            case StrictLaptopRequestStatus.Idle:
                return next == StrictLaptopRequestStatus.Warming || next == StrictLaptopRequestStatus.Stopped;
            case StrictLaptopRequestStatus.Warming:
                switch (next)
                {
                    case StrictLaptopRequestStatus.Draining:
                    case StrictLaptopRequestStatus.Stopped:
                        return true;
                }
                return false;
            case StrictLaptopRequestStatus.Ready:
                return next == StrictLaptopRequestStatus.Draining || next == StrictLaptopRequestStatus.Stopped;
            case StrictLaptopRequestStatus.Draining:
                switch (next)
                {
                    case StrictLaptopRequestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictLaptopRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict laptop request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Cost - (double)Retries, 2);
    }

    public StrictLaptopRequest Copy()
    {
        return new StrictLaptopRequest(Code, Label, Cost, Retries, Segment, Amount, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
