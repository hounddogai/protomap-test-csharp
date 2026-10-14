namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition quota is in its life cycle.</summary>
public enum RecognitionQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a recognition quota and its current status.</summary>
public sealed class RecognitionQuota(string code, string title, int sequence, string costCenter, double weight, RecognitionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public string CostCenter { get; set; } = costCenter;
    public double Weight { get; set; } = weight;
    public RecognitionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a recognition quota in status draft.</summary>
    public static RecognitionQuota NewRecognitionQuota(string code, string title)
    {
        return new RecognitionQuota(code, title, 0, "", 0.0, RecognitionQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionQuotaStatus next)
    {
        switch (Status)
        {
            case RecognitionQuotaStatus.Draft:
                switch (next)
                {
                    case RecognitionQuotaStatus.Pending:
                        return true;
                }
                return false;
            case RecognitionQuotaStatus.Pending:
                return next == RecognitionQuotaStatus.Suspended;
            case RecognitionQuotaStatus.Active:
                return next == RecognitionQuotaStatus.Suspended || next == RecognitionQuotaStatus.Closed;
            case RecognitionQuotaStatus.Suspended:
                return next == RecognitionQuotaStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RecognitionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"recognition quota"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Sequence - Weight, 2);
    }

    public RecognitionQuota Copy()
    {
        return new RecognitionQuota(Code, Title, Sequence, CostCenter, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
