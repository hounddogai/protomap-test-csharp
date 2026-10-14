namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled reorg variance is in its life cycle.</summary>
public enum ScheduledReorgVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a scheduled reorg variance and its current status.</summary>
public sealed class ScheduledReorgVariance(string code, string label, int retries, string timezone, string contactEmail, bool visible, string ownerId, string skill, string checksum, ScheduledReorgVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public string Timezone { get; set; } = timezone;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Visible { get; set; } = visible;
    public string OwnerId { get; set; } = ownerId;
    public string Skill { get; set; } = skill;
    public string Checksum { get; set; } = checksum;
    public ScheduledReorgVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled reorg variance in status requested.</summary>
    public static ScheduledReorgVariance NewScheduledReorgVariance(string code, string label)
    {
        return new ScheduledReorgVariance(
            code,
            label,
            0,
            "",
            "",
            false,
            "",
            "",
            "",
            ScheduledReorgVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledReorgVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledReorgVarianceStatus next)
    {
        switch (Status)
        {
            case ScheduledReorgVarianceStatus.Requested:
                return next == ScheduledReorgVarianceStatus.Expired;
            case ScheduledReorgVarianceStatus.Granted:
                return next == ScheduledReorgVarianceStatus.Revoked || next == ScheduledReorgVarianceStatus.Expired;
            case ScheduledReorgVarianceStatus.Revoked:
                switch (next)
                {
                    case ScheduledReorgVarianceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledReorgVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled reorg variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public ScheduledReorgVariance Copy()
    {
        return new ScheduledReorgVariance(
            Code,
            Label,
            Retries,
            Timezone,
            ContactEmail,
            Visible,
            OwnerId,
            Skill,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
