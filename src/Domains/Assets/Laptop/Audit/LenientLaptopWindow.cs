namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient laptop window is in its life cycle.</summary>
public enum LenientLaptopWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a lenient laptop window and its current status.</summary>
public sealed class LenientLaptopWindow(string code, string name, string phone, int leaveDays, string department, string region, double score, int batchSize, string contactEmail, LenientLaptopWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public int LeaveDays { get; set; } = leaveDays;
    public string Department { get; set; } = department;
    public string Region { get; set; } = region;
    public double Score { get; set; } = score;
    public int BatchSize { get; set; } = batchSize;
    public string ContactEmail { get; set; } = contactEmail;
    public LenientLaptopWindowStatus Status { get; set; } = status;

    /// <summary>Starts a lenient laptop window in status idle.</summary>
    public static LenientLaptopWindow NewLenientLaptopWindow(string code, string name)
    {
        return new LenientLaptopWindow(code, name, "", 0, "", "", 0.0, 0, "", LenientLaptopWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLaptopWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLaptopWindowStatus next)
    {
        switch (Status)
        {
            case LenientLaptopWindowStatus.Idle:
                switch (next)
                {
                    case LenientLaptopWindowStatus.Draining:
                    case LenientLaptopWindowStatus.Stopped:
                        return true;
                }
                return false;
            case LenientLaptopWindowStatus.Warming:
                switch (next)
                {
                    case LenientLaptopWindowStatus.Draining:
                    case LenientLaptopWindowStatus.Stopped:
                        return true;
                }
                return false;
            case LenientLaptopWindowStatus.Ready:
                switch (next)
                {
                    case LenientLaptopWindowStatus.Draining:
                    case LenientLaptopWindowStatus.Stopped:
                        return true;
                }
                return false;
            case LenientLaptopWindowStatus.Draining:
                switch (next)
                {
                    case LenientLaptopWindowStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientLaptopWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient laptop window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)LeaveDays * Score, 2);
    }

    public LenientLaptopWindow Copy()
    {
        return new LenientLaptopWindow(
            Code,
            Name,
            Phone,
            LeaveDays,
            Department,
            Region,
            Score,
            BatchSize,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
