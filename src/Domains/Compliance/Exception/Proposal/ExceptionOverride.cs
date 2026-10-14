namespace Acme.Workforce.Compliance.Exception.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception override is in its life cycle.</summary>
public enum ExceptionOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A exception override. Its code is unique within a tenant.</summary>
public sealed class ExceptionOverride(string code, string name, int batchSize, int startsAt, bool pinned, string phone, ExceptionOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public int StartsAt { get; set; } = startsAt;
    public bool Pinned { get; set; } = pinned;
    public string Phone { get; set; } = phone;
    public ExceptionOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a exception override in status new.</summary>
    public static ExceptionOverride NewExceptionOverride(string code, string name)
    {
        return new ExceptionOverride(code, name, 0, 0, false, "", ExceptionOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionOverrideStatus next)
    {
        switch (Status)
        {
            case ExceptionOverrideStatus.New:
                return next == ExceptionOverrideStatus.Failed;
            case ExceptionOverrideStatus.Queued:
                switch (next)
                {
                    case ExceptionOverrideStatus.Running:
                    case ExceptionOverrideStatus.Failed:
                        return true;
                }
                return false;
            case ExceptionOverrideStatus.Running:
                return next == ExceptionOverrideStatus.Done;
            case ExceptionOverrideStatus.Done:
                switch (next)
                {
                    case ExceptionOverrideStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"exception override"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)BatchSize * (double)StartsAt, 2);
    }

    public ExceptionOverride Copy()
    {
        return new ExceptionOverride(Code, Name, BatchSize, StartsAt, Pinned, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
