namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due batch is in its life cycle.</summary>
public enum TrainingDueBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A training due batch. Its code is unique within a tenant.</summary>
public sealed class TrainingDueBatch(string code, string name, string locale, string segment, double rating, int attempts, double score, double margin, string source, TrainingDueBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public double Score { get; set; } = score;
    public double Margin { get; set; } = margin;
    public string Source { get; set; } = source;
    public TrainingDueBatchStatus Status { get; set; } = status;

    /// <summary>Starts a training due batch in status requested.</summary>
    public static TrainingDueBatch NewTrainingDueBatch(string code, string name)
    {
        return new TrainingDueBatch(code, name, "", "", 0.0, 0, 0.0, 0.0, "", TrainingDueBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueBatchStatus next)
    {
        switch (Status)
        {
            case TrainingDueBatchStatus.Requested:
                return next == TrainingDueBatchStatus.Granted || next == TrainingDueBatchStatus.Expired;
            case TrainingDueBatchStatus.Granted:
                return next == TrainingDueBatchStatus.Expired;
            case TrainingDueBatchStatus.Revoked:
                switch (next)
                {
                    case TrainingDueBatchStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Margin - Score, 2);
    }

    public TrainingDueBatch Copy()
    {
        return new TrainingDueBatch(Code, Name, Locale, Segment, Rating, Attempts, Score, Margin, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
