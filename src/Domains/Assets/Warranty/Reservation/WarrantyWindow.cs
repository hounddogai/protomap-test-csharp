namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty window is in its life cycle.</summary>
public enum WarrantyWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One warranty window, with what the planners need to act on it.</summary>
public sealed class WarrantyWindow(string code, string label, string channel, int batchSize, int limit, double threshold, int retries, string jobCode, WarrantyWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int BatchSize { get; set; } = batchSize;
    public int Limit { get; set; } = limit;
    public double Threshold { get; set; } = threshold;
    public int Retries { get; set; } = retries;
    public string JobCode { get; set; } = jobCode;
    public WarrantyWindowStatus Status { get; set; } = status;

    /// <summary>Starts a warranty window in status draft.</summary>
    public static WarrantyWindow NewWarrantyWindow(string code, string label)
    {
        return new WarrantyWindow(code, label, "", 0, 0, 0.0, 0, "", WarrantyWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyWindowStatus next)
    {
        switch (Status)
        {
            case WarrantyWindowStatus.Draft:
                return next == WarrantyWindowStatus.Pending || next == WarrantyWindowStatus.Suspended;
            case WarrantyWindowStatus.Pending:
                switch (next)
                {
                    case WarrantyWindowStatus.Suspended:
                        return true;
                }
                return false;
            case WarrantyWindowStatus.Active:
                return next == WarrantyWindowStatus.Suspended || next == WarrantyWindowStatus.Closed;
            case WarrantyWindowStatus.Suspended:
                return next == WarrantyWindowStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WarrantyWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"warranty window"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Limit + Threshold, 2);
    }

    public WarrantyWindow Copy()
    {
        return new WarrantyWindow(Code, Label, Channel, BatchSize, Limit, Threshold, Retries, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
