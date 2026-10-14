namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position batch is in its life cycle.</summary>
public enum PositionBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a position batch and its current status.</summary>
public sealed class PositionBatch(string code, string title, double score, string name, int retries, PositionBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public PositionBatchStatus Status { get; set; } = status;

    /// <summary>Starts a position batch in status requested.</summary>
    public static PositionBatch NewPositionBatch(string code, string title)
    {
        return new PositionBatch(code, title, 0.0, "", 0, PositionBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionBatchStatus next)
    {
        switch (Status)
        {
            case PositionBatchStatus.Requested:
                return next == PositionBatchStatus.Granted;
            case PositionBatchStatus.Granted:
                switch (next)
                {
                    case PositionBatchStatus.Expired:
                        return true;
                }
                return false;
            case PositionBatchStatus.Revoked:
                switch (next)
                {
                    case PositionBatchStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Score - (double)Retries, 2);
    }

    public PositionBatch Copy()
    {
        return new PositionBatch(Code, Title, Score, Name, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
