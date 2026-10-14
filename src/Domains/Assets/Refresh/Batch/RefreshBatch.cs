namespace Acme.Workforce.Assets.Refresh.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh batch is in its life cycle.</summary>
public enum RefreshBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A refresh batch as the refresh team keeps it.</summary>
public sealed class RefreshBatch(string code, string label, string currency, string channel, string grade, double floorValue, int revision, double minimum, double margin, RefreshBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string Channel { get; set; } = channel;
    public string Grade { get; set; } = grade;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public double Minimum { get; set; } = minimum;
    public double Margin { get; set; } = margin;
    public RefreshBatchStatus Status { get; set; } = status;

    /// <summary>Starts a refresh batch in status requested.</summary>
    public static RefreshBatch NewRefreshBatch(string code, string label)
    {
        return new RefreshBatch(code, label, "", "", "", 0.0, 0, 0.0, 0.0, RefreshBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshBatchStatus next)
    {
        switch (Status)
        {
            case RefreshBatchStatus.Requested:
                switch (next)
                {
                    case RefreshBatchStatus.Granted:
                    case RefreshBatchStatus.Expired:
                        return true;
                }
                return false;
            case RefreshBatchStatus.Granted:
                switch (next)
                {
                    case RefreshBatchStatus.Expired:
                        return true;
                }
                return false;
            case RefreshBatchStatus.Revoked:
                return next == RefreshBatchStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RefreshBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round((double)Revision + Minimum, 2);
    }

    public RefreshBatch Copy()
    {
        return new RefreshBatch(
            Code,
            Label,
            Currency,
            Channel,
            Grade,
            FloorValue,
            Revision,
            Minimum,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
