namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered benchmark reservation is in its life cycle.</summary>
public enum TieredBenchmarkReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered benchmark reservation as the benchmark team keeps it.</summary>
public sealed class TieredBenchmarkReservation(string code, string name, string segment, double discount, bool archived, TieredBenchmarkReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Discount { get; set; } = discount;
    public bool Archived { get; set; } = archived;
    public TieredBenchmarkReservationStatus Status { get; set; } = status;

    /// <summary>Starts a tiered benchmark reservation in status draft.</summary>
    public static TieredBenchmarkReservation NewTieredBenchmarkReservation(string code, string name)
    {
        return new TieredBenchmarkReservation(code, name, "", 0.0, false, TieredBenchmarkReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBenchmarkReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBenchmarkReservationStatus next)
    {
        switch (Status)
        {
            case TieredBenchmarkReservationStatus.Draft:
                switch (next)
                {
                    case TieredBenchmarkReservationStatus.Closed:
                        return true;
                }
                return false;
            case TieredBenchmarkReservationStatus.Pending:
                return next == TieredBenchmarkReservationStatus.Suspended || next == TieredBenchmarkReservationStatus.Closed;
            case TieredBenchmarkReservationStatus.Active:
                switch (next)
                {
                    case TieredBenchmarkReservationStatus.Suspended:
                    case TieredBenchmarkReservationStatus.Closed:
                        return true;
                }
                return false;
            case TieredBenchmarkReservationStatus.Suspended:
                switch (next)
                {
                    case TieredBenchmarkReservationStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredBenchmarkReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered benchmark reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered benchmark reservation"} {Code}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public TieredBenchmarkReservation Copy()
    {
        return new TieredBenchmarkReservation(Code, Name, Segment, Discount, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
