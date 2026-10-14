namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual bundle is in its life cycle.</summary>
public enum AccrualBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A accrual bundle. Its code is unique within a tenant.</summary>
public sealed class AccrualBundle(string code, string name, int expiresAt, string channel, int graceDays, string contactEmail, string grade, int limit, double floorValue, AccrualBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public int GraceDays { get; set; } = graceDays;
    public string ContactEmail { get; set; } = contactEmail;
    public string Grade { get; set; } = grade;
    public int Limit { get; set; } = limit;
    public double FloorValue { get; set; } = floorValue;
    public AccrualBundleStatus Status { get; set; } = status;

    /// <summary>Starts a accrual bundle in status new.</summary>
    public static AccrualBundle NewAccrualBundle(string code, string name)
    {
        return new AccrualBundle(code, name, 0, "", 0, "", "", 0, 0.0, AccrualBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualBundleStatus next)
    {
        switch (Status)
        {
            case AccrualBundleStatus.New:
                return next == AccrualBundleStatus.Queued || next == AccrualBundleStatus.Done;
            case AccrualBundleStatus.Queued:
                switch (next)
                {
                    case AccrualBundleStatus.Running:
                        return true;
                }
                return false;
            case AccrualBundleStatus.Running:
                return next == AccrualBundleStatus.Done || next == AccrualBundleStatus.Failed;
            case AccrualBundleStatus.Done:
                return next == AccrualBundleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AccrualBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"accrual bundle"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(FloorValue * (double)ExpiresAt, 2);
    }

    public AccrualBundle Copy()
    {
        return new AccrualBundle(
            Code,
            Name,
            ExpiresAt,
            Channel,
            GraceDays,
            ContactEmail,
            Grade,
            Limit,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
