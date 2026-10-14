namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary span score is in its life cycle.</summary>
public enum PrimarySpanScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One primary span score, with what the planners need to act on it.</summary>
public sealed class PrimarySpanScore(string code, string label, string checksum, string costCenter, string grade, int createdAt, int retries, string notes, string phone, int batchSize, PrimarySpanScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string CostCenter { get; set; } = costCenter;
    public string Grade { get; set; } = grade;
    public int CreatedAt { get; set; } = createdAt;
    public int Retries { get; set; } = retries;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public int BatchSize { get; set; } = batchSize;
    public PrimarySpanScoreStatus Status { get; set; } = status;

    /// <summary>Starts a primary span score in status draft.</summary>
    public static PrimarySpanScore NewPrimarySpanScore(string code, string label)
    {
        return new PrimarySpanScore(code, label, "", "", "", 0, 0, "", "", 0, PrimarySpanScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySpanScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySpanScoreStatus next)
    {
        switch (Status)
        {
            case PrimarySpanScoreStatus.Draft:
                switch (next)
                {
                    case PrimarySpanScoreStatus.Pending:
                    case PrimarySpanScoreStatus.Closed:
                        return true;
                }
                return false;
            case PrimarySpanScoreStatus.Pending:
                return next == PrimarySpanScoreStatus.Active;
            case PrimarySpanScoreStatus.Active:
                switch (next)
                {
                    case PrimarySpanScoreStatus.Suspended:
                    case PrimarySpanScoreStatus.Closed:
                        return true;
                }
                return false;
            case PrimarySpanScoreStatus.Suspended:
                return next == PrimarySpanScoreStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimarySpanScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary span score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)BatchSize + (double)Retries, 2);
    }

    public PrimarySpanScore Copy()
    {
        return new PrimarySpanScore(
            Code,
            Label,
            Checksum,
            CostCenter,
            Grade,
            CreatedAt,
            Retries,
            Notes,
            Phone,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
