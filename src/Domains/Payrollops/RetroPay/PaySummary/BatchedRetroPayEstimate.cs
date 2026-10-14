namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched retro pay estimate is in its life cycle.</summary>
public enum BatchedRetroPayEstimateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A batched retro pay estimate as the retro pay team keeps it.</summary>
public sealed class BatchedRetroPayEstimate(string code, string title, double fte, int leaveDays, double minimum, double margin, double ceiling, BatchedRetroPayEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public int LeaveDays { get; set; } = leaveDays;
    public double Minimum { get; set; } = minimum;
    public double Margin { get; set; } = margin;
    public double Ceiling { get; set; } = ceiling;
    public BatchedRetroPayEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a batched retro pay estimate in status created.</summary>
    public static BatchedRetroPayEstimate NewBatchedRetroPayEstimate(string code, string title)
    {
        return new BatchedRetroPayEstimate(
            code,
            title,
            0.0,
            0,
            0.0,
            0.0,
            0.0,
            BatchedRetroPayEstimateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRetroPayEstimateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRetroPayEstimateStatus next)
    {
        switch (Status)
        {
            case BatchedRetroPayEstimateStatus.Created:
                return next == BatchedRetroPayEstimateStatus.Published || next == BatchedRetroPayEstimateStatus.Retired;
            case BatchedRetroPayEstimateStatus.Validated:
                return next == BatchedRetroPayEstimateStatus.Published;
            case BatchedRetroPayEstimateStatus.Published:
                switch (next)
                {
                    case BatchedRetroPayEstimateStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedRetroPayEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched retro pay estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Fte + Margin, 2);
    }

    public BatchedRetroPayEstimate Copy()
    {
        return new BatchedRetroPayEstimate(Code, Title, Fte, LeaveDays, Minimum, Margin, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
