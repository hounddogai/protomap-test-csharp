namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop override is in its life cycle.</summary>
public enum LaptopOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a laptop override and its current status.</summary>
public sealed class LaptopOverride(string code, string title, double threshold, int version, string employeeId, int revision, LaptopOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public int Version { get; set; } = version;
    public string EmployeeId { get; set; } = employeeId;
    public int Revision { get; set; } = revision;
    public LaptopOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a laptop override in status requested.</summary>
    public static LaptopOverride NewLaptopOverride(string code, string title)
    {
        return new LaptopOverride(code, title, 0.0, 0, "", 0, LaptopOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopOverrideStatus next)
    {
        switch (Status)
        {
            case LaptopOverrideStatus.Requested:
                return next == LaptopOverrideStatus.Granted;
            case LaptopOverrideStatus.Granted:
                switch (next)
                {
                    case LaptopOverrideStatus.Revoked:
                    case LaptopOverrideStatus.Expired:
                        return true;
                }
                return false;
            case LaptopOverrideStatus.Revoked:
                switch (next)
                {
                    case LaptopOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"laptop override"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Threshold * (double)Revision, 2);
    }

    public LaptopOverride Copy()
    {
        return new LaptopOverride(Code, Title, Threshold, Version, EmployeeId, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
