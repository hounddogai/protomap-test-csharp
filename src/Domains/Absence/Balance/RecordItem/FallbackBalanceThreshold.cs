namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback balance threshold is in its life cycle.</summary>
public enum FallbackBalanceThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback balance threshold as the balance team keeps it.</summary>
public sealed class FallbackBalanceThreshold(string code, string title, int retries, int batchSize, double fte, string checksum, string category, FallbackBalanceThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public int BatchSize { get; set; } = batchSize;
    public double Fte { get; set; } = fte;
    public string Checksum { get; set; } = checksum;
    public string Category { get; set; } = category;
    public FallbackBalanceThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a fallback balance threshold in status proposed.</summary>
    public static FallbackBalanceThreshold NewFallbackBalanceThreshold(string code, string title)
    {
        return new FallbackBalanceThreshold(code, title, 0, 0, 0.0, "", "", FallbackBalanceThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBalanceThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBalanceThresholdStatus next)
    {
        switch (Status)
        {
            case FallbackBalanceThresholdStatus.Proposed:
                switch (next)
                {
                    case FallbackBalanceThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackBalanceThresholdStatus.Approved:
                switch (next)
                {
                    case FallbackBalanceThresholdStatus.Rejected:
                    case FallbackBalanceThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackBalanceThresholdStatus.Rejected:
                switch (next)
                {
                    case FallbackBalanceThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackBalanceThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback balance threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback balance threshold"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)BatchSize + Fte, 2);
    }

    public FallbackBalanceThreshold Copy()
    {
        return new FallbackBalanceThreshold(Code, Title, Retries, BatchSize, Fte, Checksum, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
