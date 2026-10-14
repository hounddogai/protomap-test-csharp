namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work estimate is in its life cycle.</summary>
public enum StatementOfWorkEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A statement of work estimate. Its code is unique within a tenant.</summary>
public sealed class StatementOfWorkEstimate(string code, string label, double score, string checksum, int attempts, string description, int startsAt, Dictionary<string, string> attributes, double ceiling, string ownerId, StatementOfWorkEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public string Checksum { get; set; } = checksum;
    public int Attempts { get; set; } = attempts;
    public string Description { get; set; } = description;
    public int StartsAt { get; set; } = startsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ceiling { get; set; } = ceiling;
    public string OwnerId { get; set; } = ownerId;
    public StatementOfWorkEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work estimate in status draft.</summary>
    public static StatementOfWorkEstimate NewStatementOfWorkEstimate(string code, string label)
    {
        return new StatementOfWorkEstimate(
            code,
            label,
            0.0,
            "",
            0,
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            "",
            StatementOfWorkEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkEstimateStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkEstimateStatus.Draft:
                switch (next)
                {
                    case StatementOfWorkEstimateStatus.Suspended:
                        return true;
                }
                return false;
            case StatementOfWorkEstimateStatus.Pending:
                switch (next)
                {
                    case StatementOfWorkEstimateStatus.Active:
                    case StatementOfWorkEstimateStatus.Closed:
                        return true;
                }
                return false;
            case StatementOfWorkEstimateStatus.Active:
                return next == StatementOfWorkEstimateStatus.Suspended || next == StatementOfWorkEstimateStatus.Closed;
            case StatementOfWorkEstimateStatus.Suspended:
                switch (next)
                {
                    case StatementOfWorkEstimateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveLoad()
    {
        return Math.Round(Score * (double)StartsAt, 2);
    }

    public StatementOfWorkEstimate Copy()
    {
        return new StatementOfWorkEstimate(
            Code,
            Label,
            Score,
            Checksum,
            Attempts,
            Description,
            StartsAt,
            Attributes,
            Ceiling,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
