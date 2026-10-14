namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pay run request is in its life cycle.</summary>
public enum DefaultPayRunRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default pay run request. Its code is unique within a tenant.</summary>
public sealed class DefaultPayRunRequest(string code, string label, string channel, string salaryBand, string externalRef, int version, int windowDays, double fte, DefaultPayRunRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string SalaryBand { get; set; } = salaryBand;
    public string ExternalRef { get; set; } = externalRef;
    public int Version { get; set; } = version;
    public int WindowDays { get; set; } = windowDays;
    public double Fte { get; set; } = fte;
    public DefaultPayRunRequestStatus Status { get; set; } = status;

    /// <summary>Starts a default pay run request in status planned.</summary>
    public static DefaultPayRunRequest NewDefaultPayRunRequest(string code, string label)
    {
        return new DefaultPayRunRequest(code, label, "", "", "", 0, 0, 0.0, DefaultPayRunRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayRunRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayRunRequestStatus next)
    {
        switch (Status)
        {
            case DefaultPayRunRequestStatus.Planned:
                return next == DefaultPayRunRequestStatus.Scheduled || next == DefaultPayRunRequestStatus.Cancelled;
            case DefaultPayRunRequestStatus.Scheduled:
                return next == DefaultPayRunRequestStatus.Cancelled;
            case DefaultPayRunRequestStatus.InProgress:
                return next == DefaultPayRunRequestStatus.Cancelled;
            case DefaultPayRunRequestStatus.Completed:
                switch (next)
                {
                    case DefaultPayRunRequestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPayRunRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pay run request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"default pay run request"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Version * (double)WindowDays, 2);
    }

    public DefaultPayRunRequest Copy()
    {
        return new DefaultPayRunRequest(
            Code,
            Label,
            Channel,
            SalaryBand,
            ExternalRef,
            Version,
            WindowDays,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
