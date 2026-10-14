namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark reservation is in its life cycle.</summary>
public enum BenchmarkReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A benchmark reservation. Its code is unique within a tenant.</summary>
public sealed class BenchmarkReservation(string code, string title, Dictionary<string, string> attributes, int windowDays, string location, bool archived, double score, BenchmarkReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int WindowDays { get; set; } = windowDays;
    public string Location { get; set; } = location;
    public bool Archived { get; set; } = archived;
    public double Score { get; set; } = score;
    public BenchmarkReservationStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark reservation in status draft.</summary>
    public static BenchmarkReservation NewBenchmarkReservation(string code, string title)
    {
        return new BenchmarkReservation(
            code,
            title,
            new Dictionary<string, string>(),
            0,
            "",
            false,
            0.0,
            BenchmarkReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkReservationStatus next)
    {
        switch (Status)
        {
            case BenchmarkReservationStatus.Draft:
                switch (next)
                {
                    case BenchmarkReservationStatus.Suspended:
                    case BenchmarkReservationStatus.Closed:
                        return true;
                }
                return false;
            case BenchmarkReservationStatus.Pending:
                return next == BenchmarkReservationStatus.Suspended;
            case BenchmarkReservationStatus.Active:
                switch (next)
                {
                    case BenchmarkReservationStatus.Suspended:
                    case BenchmarkReservationStatus.Closed:
                        return true;
                }
                return false;
            case BenchmarkReservationStatus.Suspended:
                return next == BenchmarkReservationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BenchmarkReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedTotal()
    {
        return Math.Round(Score * (double)WindowDays, 2);
    }

    public BenchmarkReservation Copy()
    {
        return new BenchmarkReservation(Code, Title, Attributes, WindowDays, Location, Archived, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
