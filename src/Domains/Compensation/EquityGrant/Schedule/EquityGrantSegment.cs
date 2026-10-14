namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant segment is in its life cycle.</summary>
public enum EquityGrantSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A equity grant segment. Its code is unique within a tenant.</summary>
public sealed class EquityGrantSegment(string code, string name, int endsAt, bool pinned, string externalRef, string employeeId, string locale, int version, string unit, EquityGrantSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public bool Pinned { get; set; } = pinned;
    public string ExternalRef { get; set; } = externalRef;
    public string EmployeeId { get; set; } = employeeId;
    public string Locale { get; set; } = locale;
    public int Version { get; set; } = version;
    public string Unit { get; set; } = unit;
    public EquityGrantSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant segment in status new.</summary>
    public static EquityGrantSegment NewEquityGrantSegment(string code, string name)
    {
        return new EquityGrantSegment(code, name, 0, false, "", "", "", 0, "", EquityGrantSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantSegmentStatus next)
    {
        switch (Status)
        {
            case EquityGrantSegmentStatus.New:
                return next == EquityGrantSegmentStatus.Queued;
            case EquityGrantSegmentStatus.Queued:
                switch (next)
                {
                    case EquityGrantSegmentStatus.Done:
                    case EquityGrantSegmentStatus.Failed:
                        return true;
                }
                return false;
            case EquityGrantSegmentStatus.Running:
                return next == EquityGrantSegmentStatus.Done || next == EquityGrantSegmentStatus.Failed;
            case EquityGrantSegmentStatus.Done:
                switch (next)
                {
                    case EquityGrantSegmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)EndsAt + (double)Version, 2);
    }

    public EquityGrantSegment Copy()
    {
        return new EquityGrantSegment(
            Code,
            Name,
            EndsAt,
            Pinned,
            ExternalRef,
            EmployeeId,
            Locale,
            Version,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
