namespace Acme.Workforce.Payrollops.PayRun.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run request is in its life cycle.</summary>
public enum PayRunRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a pay run request and its current status.</summary>
public sealed class PayRunRequest(string code, string label, double score, int expiresAt, double budget, string badge, double rating, bool pinned, int batchSize, PayRunRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Budget { get; set; } = budget;
    public string Badge { get; set; } = badge;
    public double Rating { get; set; } = rating;
    public bool Pinned { get; set; } = pinned;
    public int BatchSize { get; set; } = batchSize;
    public PayRunRequestStatus Status { get; set; } = status;

    /// <summary>Starts a pay run request in status new.</summary>
    public static PayRunRequest NewPayRunRequest(string code, string label)
    {
        return new PayRunRequest(code, label, 0.0, 0, 0.0, "", 0.0, false, 0, PayRunRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunRequestStatus next)
    {
        switch (Status)
        {
            case PayRunRequestStatus.New:
                switch (next)
                {
                    case PayRunRequestStatus.Queued:
                    case PayRunRequestStatus.Failed:
                        return true;
                }
                return false;
            case PayRunRequestStatus.Queued:
                return next == PayRunRequestStatus.Running || next == PayRunRequestStatus.Failed;
            case PayRunRequestStatus.Running:
                return next == PayRunRequestStatus.Done || next == PayRunRequestStatus.Failed;
            case PayRunRequestStatus.Done:
                switch (next)
                {
                    case PayRunRequestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)BatchSize - Rating, 2);
    }

    public PayRunRequest Copy()
    {
        return new PayRunRequest(Code, Label, Score, ExpiresAt, Budget, Badge, Rating, Pinned, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
