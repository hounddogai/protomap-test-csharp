namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered rate window is in its life cycle.</summary>
public enum TieredRateWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered rate window. Its code is unique within a tenant.</summary>
public sealed class TieredRateWindow(string code, string name, string source, bool visible, double score, string location, string employeeId, int version, double fte, string grade, string phone, TieredRateWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public bool Visible { get; set; } = visible;
    public double Score { get; set; } = score;
    public string Location { get; set; } = location;
    public string EmployeeId { get; set; } = employeeId;
    public int Version { get; set; } = version;
    public double Fte { get; set; } = fte;
    public string Grade { get; set; } = grade;
    public string Phone { get; set; } = phone;
    public TieredRateWindowStatus Status { get; set; } = status;

    /// <summary>Starts a tiered rate window in status draft.</summary>
    public static TieredRateWindow NewTieredRateWindow(string code, string name)
    {
        return new TieredRateWindow(
            code,
            name,
            "",
            false,
            0.0,
            "",
            "",
            0,
            0.0,
            "",
            "",
            TieredRateWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredRateWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredRateWindowStatus next)
    {
        switch (Status)
        {
            case TieredRateWindowStatus.Draft:
                switch (next)
                {
                    case TieredRateWindowStatus.Active:
                        return true;
                }
                return false;
            case TieredRateWindowStatus.Pending:
                switch (next)
                {
                    case TieredRateWindowStatus.Active:
                    case TieredRateWindowStatus.Closed:
                        return true;
                }
                return false;
            case TieredRateWindowStatus.Active:
                return next == TieredRateWindowStatus.Closed;
            case TieredRateWindowStatus.Suspended:
                switch (next)
                {
                    case TieredRateWindowStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredRateWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered rate window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"tiered rate window"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Version * Fte, 2);
    }

    public TieredRateWindow Copy()
    {
        return new TieredRateWindow(
            Code,
            Name,
            Source,
            Visible,
            Score,
            Location,
            EmployeeId,
            Version,
            Fte,
            Grade,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
