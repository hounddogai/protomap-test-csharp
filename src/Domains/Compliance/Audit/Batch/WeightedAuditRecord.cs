namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted audit record is in its life cycle.</summary>
public enum WeightedAuditRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One weighted audit record, with what the planners need to act on it.</summary>
public sealed class WeightedAuditRecord(string code, string label, int attempts, string reviewer, string team, int sequence, string locale, double threshold, WeightedAuditRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public string Reviewer { get; set; } = reviewer;
    public string Team { get; set; } = team;
    public int Sequence { get; set; } = sequence;
    public string Locale { get; set; } = locale;
    public double Threshold { get; set; } = threshold;
    public WeightedAuditRecordStatus Status { get; set; } = status;

    /// <summary>Starts a weighted audit record in status draft.</summary>
    public static WeightedAuditRecord NewWeightedAuditRecord(string code, string label)
    {
        return new WeightedAuditRecord(code, label, 0, "", "", 0, "", 0.0, WeightedAuditRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAuditRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAuditRecordStatus next)
    {
        switch (Status)
        {
            case WeightedAuditRecordStatus.Draft:
                return next == WeightedAuditRecordStatus.Pending || next == WeightedAuditRecordStatus.Suspended;
            case WeightedAuditRecordStatus.Pending:
                return next == WeightedAuditRecordStatus.Active || next == WeightedAuditRecordStatus.Suspended;
            case WeightedAuditRecordStatus.Active:
                switch (next)
                {
                    case WeightedAuditRecordStatus.Closed:
                        return true;
                }
                return false;
            case WeightedAuditRecordStatus.Suspended:
                switch (next)
                {
                    case WeightedAuditRecordStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedAuditRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted audit record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted audit record"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Sequence * Threshold, 2);
    }

    public WeightedAuditRecord Copy()
    {
        return new WeightedAuditRecord(Code, Label, Attempts, Reviewer, Team, Sequence, Locale, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
