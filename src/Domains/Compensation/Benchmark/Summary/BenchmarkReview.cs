namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark review is in its life cycle.</summary>
public enum BenchmarkReviewStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A benchmark review as the benchmark team keeps it.</summary>
public sealed class BenchmarkReview(string code, string label, string title, bool visible, double discount, int windowDays, string location, string department, double margin, BenchmarkReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public double Discount { get; set; } = discount;
    public int WindowDays { get; set; } = windowDays;
    public string Location { get; set; } = location;
    public string Department { get; set; } = department;
    public double Margin { get; set; } = margin;
    public BenchmarkReviewStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark review in status draft.</summary>
    public static BenchmarkReview NewBenchmarkReview(string code, string label)
    {
        return new BenchmarkReview(code, label, "", false, 0.0, 0, "", "", 0.0, BenchmarkReviewStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkReviewStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkReviewStatus next)
    {
        switch (Status)
        {
            case BenchmarkReviewStatus.Draft:
                return next == BenchmarkReviewStatus.Pending || next == BenchmarkReviewStatus.Suspended;
            case BenchmarkReviewStatus.Pending:
                return next == BenchmarkReviewStatus.Closed;
            case BenchmarkReviewStatus.Active:
                switch (next)
                {
                    case BenchmarkReviewStatus.Suspended:
                    case BenchmarkReviewStatus.Closed:
                        return true;
                }
                return false;
            case BenchmarkReviewStatus.Suspended:
                switch (next)
                {
                    case BenchmarkReviewStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"benchmark review"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)WindowDays * Discount, 2);
    }

    public BenchmarkReview Copy()
    {
        return new BenchmarkReview(
            Code,
            Label,
            Title,
            Visible,
            Discount,
            WindowDays,
            Location,
            Department,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
