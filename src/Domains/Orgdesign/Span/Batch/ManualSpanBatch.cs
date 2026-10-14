namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual span batch is in its life cycle.</summary>
public enum ManualSpanBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A manual span batch. Its code is unique within a tenant.</summary>
public sealed class ManualSpanBatch(string code, string title, double ratio, string channel, int leaveDays, string country, int updatedAt, ManualSpanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Channel { get; set; } = channel;
    public int LeaveDays { get; set; } = leaveDays;
    public string Country { get; set; } = country;
    public int UpdatedAt { get; set; } = updatedAt;
    public ManualSpanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a manual span batch in status new.</summary>
    public static ManualSpanBatch NewManualSpanBatch(string code, string title)
    {
        return new ManualSpanBatch(code, title, 0.0, "", 0, "", 0, ManualSpanBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualSpanBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualSpanBatchStatus next)
    {
        switch (Status)
        {
            case ManualSpanBatchStatus.New:
                return next == ManualSpanBatchStatus.Running || next == ManualSpanBatchStatus.Failed;
            case ManualSpanBatchStatus.Queued:
                return next == ManualSpanBatchStatus.Running || next == ManualSpanBatchStatus.Failed;
            case ManualSpanBatchStatus.Running:
                switch (next)
                {
                    case ManualSpanBatchStatus.Done:
                        return true;
                }
                return false;
            case ManualSpanBatchStatus.Done:
                return next == ManualSpanBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualSpanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual span batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"manual span batch"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)UpdatedAt * (double)LeaveDays, 2);
    }

    public ManualSpanBatch Copy()
    {
        return new ManualSpanBatch(Code, Title, Ratio, Channel, LeaveDays, Country, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
